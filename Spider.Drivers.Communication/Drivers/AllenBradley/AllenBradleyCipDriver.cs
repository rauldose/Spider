using System.Text;
using Spider.Drivers.Communication.Interfaces;
using Spider.Drivers.Communication.Models;

namespace Spider.Drivers.Communication.Drivers.AllenBradley;

/// <summary>
/// Allen-Bradley CIP (EtherNet/IP) driver implementation
/// </summary>
public class AllenBradleyCipDriver : PlcDriverBase
{
    private readonly AllenBradleyCipSettings _cipSettings;
    private uint _sessionHandle;
    private uint _contextId;

    public override string DriverTypeName => "AllenBradleyCIP";

    public AllenBradleyCipDriver(AllenBradleyCipSettings settings) : base(settings)
    {
        _cipSettings = settings;
    }

    #region CIP Protocol Constants

    private const ushort CommandRegisterSession = 0x0065;
    private const ushort CommandUnRegisterSession = 0x0066;
    private const ushort CommandSendRRData = 0x006F;

    private const byte ServiceReadTag = 0x4C;
    private const byte ServiceWriteTag = 0x4D;
    private const byte ServiceReadTagFragmented = 0x52;
    private const byte ServiceWriteTagFragmented = 0x53;

    #endregion

    #region Connection Methods

    protected override async Task<OperationResult> InitializeConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Register Session
            var registerResult = await RegisterSessionAsync(cancellationToken);
            if (!registerResult.IsSuccess)
            {
                return registerResult;
            }

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"CIP initialization failed: {ex.Message}");
        }
    }

    private async Task<OperationResult> RegisterSessionAsync(CancellationToken cancellationToken)
    {
        // EtherNet/IP encapsulation header for Register Session
        var request = new byte[28];
        
        // Command: Register Session (0x0065)
        request[0] = (byte)(CommandRegisterSession & 0xFF);
        request[1] = (byte)((CommandRegisterSession >> 8) & 0xFF);
        
        // Length of data (4 bytes)
        request[2] = 0x04;
        request[3] = 0x00;
        
        // Session handle (0 for registration)
        request[4] = 0x00;
        request[5] = 0x00;
        request[6] = 0x00;
        request[7] = 0x00;
        
        // Status (0)
        request[8] = 0x00;
        request[9] = 0x00;
        request[10] = 0x00;
        request[11] = 0x00;
        
        // Sender context (8 bytes)
        BitConverter.GetBytes(_contextId++).CopyTo(request, 12);
        
        // Options (0)
        request[20] = 0x00;
        request[21] = 0x00;
        request[22] = 0x00;
        request[23] = 0x00;
        
        // Protocol version (1)
        request[24] = 0x01;
        request[25] = 0x00;
        
        // Options flags (0)
        request[26] = 0x00;
        request[27] = 0x00;

        var response = await SendAndReceiveAsync(request, 28, cancellationToken);

        if (response == null || response.Length < 8)
        {
            return OperationResult.Failure("Register session failed - no response");
        }

        // Check status
        var status = BitConverter.ToUInt32(response, 8);
        if (status != 0)
        {
            return OperationResult.Failure($"Register session failed with status: {status}", (int)status);
        }

        // Extract session handle
        _sessionHandle = BitConverter.ToUInt32(response, 4);

        return OperationResult.Success();
    }

    public override async Task<OperationResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (IsConnected && _sessionHandle != 0)
            {
                await UnregisterSessionAsync(cancellationToken);
            }
        }
        catch
        {
            // Ignore errors during disconnect
        }

        _sessionHandle = 0;
        return await base.DisconnectAsync(cancellationToken);
    }

    private async Task UnregisterSessionAsync(CancellationToken cancellationToken)
    {
        var request = new byte[24];
        
        // Command: UnRegister Session (0x0066)
        request[0] = (byte)(CommandUnRegisterSession & 0xFF);
        request[1] = (byte)((CommandUnRegisterSession >> 8) & 0xFF);
        
        // Length (0)
        request[2] = 0x00;
        request[3] = 0x00;
        
        // Session handle
        BitConverter.GetBytes(_sessionHandle).CopyTo(request, 4);
        
        // Status
        request[8] = 0x00;
        request[9] = 0x00;
        request[10] = 0x00;
        request[11] = 0x00;
        
        // Sender context
        BitConverter.GetBytes(_contextId++).CopyTo(request, 12);
        
        // Options
        request[20] = 0x00;
        request[21] = 0x00;
        request[22] = 0x00;
        request[23] = 0x00;

        try
        {
            await _stream!.WriteAsync(request, cancellationToken);
            await _stream.FlushAsync(cancellationToken);
        }
        catch
        {
            // Ignore
        }
    }

    #endregion

    #region CIP Read/Write Methods

    private async Task<OperationResult<byte[]>> ReadTagAsync(string tagName, ushort elementCount, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            return OperationResult<byte[]>.Failure("Not connected");
        }

        try
        {
            var request = BuildCipReadRequest(tagName, elementCount);
            var response = await SendAndReceiveVariableLengthAsync(request, GetEncapsulationLength, 24, cancellationToken);

            if (response == null)
            {
                return OperationResult<byte[]>.Failure("No response received");
            }

            return ParseCipReadResponse(response);
        }
        catch (Exception ex)
        {
            return OperationResult<byte[]>.Failure($"Read failed: {ex.Message}");
        }
    }

    private async Task<OperationResult> WriteTagAsync(string tagName, byte[] data, ushort dataType, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            return OperationResult.Failure("Not connected");
        }

        try
        {
            var request = BuildCipWriteRequest(tagName, data, dataType);
            var response = await SendAndReceiveVariableLengthAsync(request, GetEncapsulationLength, 24, cancellationToken);

            if (response == null)
            {
                return OperationResult.Failure("No response received");
            }

            return ParseCipWriteResponse(response);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Write failed: {ex.Message}");
        }
    }

    private byte[] BuildCipReadRequest(string tagName, ushort elementCount)
    {
        var tagNameBytes = Encoding.ASCII.GetBytes(tagName);
        var tagNameLength = (byte)tagNameBytes.Length;
        var paddedTagNameLength = tagNameLength + (tagNameLength % 2); // Pad to even length

        // CIP Read Tag Service request
        var cipRequest = new List<byte>();
        cipRequest.Add(ServiceReadTag); // Service code
        cipRequest.Add((byte)(paddedTagNameLength / 2 + 1)); // Path size in words
        cipRequest.Add(0x91); // Symbolic segment
        cipRequest.Add(tagNameLength);
        cipRequest.AddRange(tagNameBytes);
        if (tagNameLength % 2 == 1)
        {
            cipRequest.Add(0x00); // Padding
        }
        cipRequest.Add((byte)(elementCount & 0xFF));
        cipRequest.Add((byte)((elementCount >> 8) & 0xFF));

        return BuildEncapsulatedRequest(cipRequest.ToArray());
    }

    private byte[] BuildCipWriteRequest(string tagName, byte[] data, ushort dataType)
    {
        var tagNameBytes = Encoding.ASCII.GetBytes(tagName);
        var tagNameLength = (byte)tagNameBytes.Length;
        var paddedTagNameLength = tagNameLength + (tagNameLength % 2);

        // CIP Write Tag Service request
        var cipRequest = new List<byte>();
        cipRequest.Add(ServiceWriteTag); // Service code
        cipRequest.Add((byte)(paddedTagNameLength / 2 + 1)); // Path size in words
        cipRequest.Add(0x91); // Symbolic segment
        cipRequest.Add(tagNameLength);
        cipRequest.AddRange(tagNameBytes);
        if (tagNameLength % 2 == 1)
        {
            cipRequest.Add(0x00); // Padding
        }
        cipRequest.Add((byte)(dataType & 0xFF)); // Data type
        cipRequest.Add((byte)((dataType >> 8) & 0xFF));
        cipRequest.Add(0x01); // Element count (low)
        cipRequest.Add(0x00); // Element count (high)
        cipRequest.AddRange(data);

        return BuildEncapsulatedRequest(cipRequest.ToArray());
    }

    private byte[] BuildEncapsulatedRequest(byte[] cipData)
    {
        // Build Common Packet Format (CPF)
        var cpf = new List<byte>();
        cpf.Add(0x00); // Item count (low)
        cpf.Add(0x02); // Item count (2 items)
        
        // Null Address Item
        cpf.Add(0x00); // Type code (low)
        cpf.Add(0x00); // Type code (high)
        cpf.Add(0x00); // Length (low)
        cpf.Add(0x00); // Length (high)
        
        // Unconnected Data Item
        cpf.Add(0xB2); // Type code (low)
        cpf.Add(0x00); // Type code (high)
        cpf.Add((byte)(cipData.Length & 0xFF)); // Length (low)
        cpf.Add((byte)((cipData.Length >> 8) & 0xFF)); // Length (high)
        cpf.AddRange(cipData);

        // Build SendRRData encapsulation
        var request = new byte[24 + cpf.Count];
        
        // Command: SendRRData (0x006F)
        request[0] = (byte)(CommandSendRRData & 0xFF);
        request[1] = (byte)((CommandSendRRData >> 8) & 0xFF);
        
        // Length
        var dataLength = (ushort)cpf.Count;
        request[2] = (byte)(dataLength & 0xFF);
        request[3] = (byte)((dataLength >> 8) & 0xFF);
        
        // Session handle
        BitConverter.GetBytes(_sessionHandle).CopyTo(request, 4);
        
        // Status
        request[8] = 0x00;
        request[9] = 0x00;
        request[10] = 0x00;
        request[11] = 0x00;
        
        // Sender context
        BitConverter.GetBytes(_contextId++).CopyTo(request, 12);
        
        // Options
        request[20] = 0x00;
        request[21] = 0x00;
        request[22] = 0x00;
        request[23] = 0x00;
        
        // CPF data
        cpf.ToArray().CopyTo(request, 24);

        return request;
    }

    private static int GetEncapsulationLength(byte[] header, int headerLength)
    {
        if (headerLength >= 4)
        {
            return 24 + ((header[2]) | (header[3] << 8));
        }
        return headerLength;
    }

    private static OperationResult<byte[]> ParseCipReadResponse(byte[] response)
    {
        if (response.Length < 44)
        {
            return OperationResult<byte[]>.Failure("Response too short");
        }

        // Check encapsulation status
        var status = BitConverter.ToUInt32(response, 8);
        if (status != 0)
        {
            return OperationResult<byte[]>.Failure($"Encapsulation error: {status}", (int)status);
        }

        // Find CIP response in CPF
        var cpfOffset = 24;
        var itemCount = BitConverter.ToUInt16(response, cpfOffset);
        
        // Skip to unconnected data item
        var offset = cpfOffset + 2;
        for (int i = 0; i < itemCount; i++)
        {
            var typeCode = BitConverter.ToUInt16(response, offset);
            var length = BitConverter.ToUInt16(response, offset + 2);
            
            if (typeCode == 0x00B2) // Unconnected Data Item
            {
                var cipOffset = offset + 4;
                var cipStatus = response[cipOffset + 2];
                
                if (cipStatus != 0)
                {
                    var extStatus = response[cipOffset + 3];
                    return OperationResult<byte[]>.Failure($"CIP error: {cipStatus}, ext: {extStatus}", cipStatus);
                }

                // Extract data (skip service reply + status + data type)
                var dataOffset = cipOffset + 6;
                var dataLength = length - 6;
                
                if (dataLength > 0 && dataOffset + dataLength <= response.Length)
                {
                    var data = new byte[dataLength];
                    Array.Copy(response, dataOffset, data, 0, dataLength);
                    return OperationResult<byte[]>.Success(data);
                }
            }
            
            offset += 4 + length;
        }

        return OperationResult<byte[]>.Failure("Invalid response format");
    }

    private static OperationResult ParseCipWriteResponse(byte[] response)
    {
        if (response.Length < 44)
        {
            return OperationResult.Failure("Response too short");
        }

        // Check encapsulation status
        var status = BitConverter.ToUInt32(response, 8);
        if (status != 0)
        {
            return OperationResult.Failure($"Encapsulation error: {status}", (int)status);
        }

        // Find CIP response
        var cpfOffset = 24;
        var offset = cpfOffset + 2;
        var typeCode = BitConverter.ToUInt16(response, offset + 4);
        
        if (response.Length > offset + 8)
        {
            // Skip to unconnected data
            var dataItemOffset = offset + 4;
            for (int i = 0; i < 2; i++)
            {
                var tc = BitConverter.ToUInt16(response, dataItemOffset);
                var len = BitConverter.ToUInt16(response, dataItemOffset + 2);
                if (tc == 0x00B2)
                {
                    var cipOffset = dataItemOffset + 4;
                    if (cipOffset + 2 < response.Length)
                    {
                        var cipStatus = response[cipOffset + 2];
                        if (cipStatus != 0)
                        {
                            return OperationResult.Failure($"CIP error: {cipStatus}", cipStatus);
                        }
                    }
                    break;
                }
                dataItemOffset += 4 + len;
            }
        }

        return OperationResult.Success();
    }

    #endregion

    #region CIP Data Types

    private const ushort CipTypeBool = 0x00C1;
    private const ushort CipTypeSint = 0x00C2; // 8-bit signed
    private const ushort CipTypeInt = 0x00C3; // 16-bit signed
    private const ushort CipTypeDint = 0x00C4; // 32-bit signed
    private const ushort CipTypeLint = 0x00C5; // 64-bit signed
    private const ushort CipTypeUsint = 0x00C6; // 8-bit unsigned
    private const ushort CipTypeUint = 0x00C7; // 16-bit unsigned
    private const ushort CipTypeUdint = 0x00C8; // 32-bit unsigned
    private const ushort CipTypeUlint = 0x00C9; // 64-bit unsigned
    private const ushort CipTypeReal = 0x00CA; // 32-bit float
    private const ushort CipTypeLreal = 0x00CB; // 64-bit float
    private const ushort CipTypeString = 0x00D0;

    #endregion

    #region Read Operations

    public override async Task<OperationResult<bool>> ReadBoolAsync(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length > 0)
        {
            return OperationResult<bool>.Success(result.Value[0] != 0);
        }
        return OperationResult<bool>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<bool[]>> ReadBoolArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var bools = new bool[Math.Min(length, result.Value.Length)];
            for (int i = 0; i < bools.Length; i++)
            {
                bools[i] = result.Value[i] != 0;
            }
            return OperationResult<bool[]>.Success(bools);
        }
        return OperationResult<bool[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<byte>> ReadByteAsync(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length > 0)
        {
            return OperationResult<byte>.Success(result.Value[0]);
        }
        return OperationResult<byte>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<byte[]>> ReadByteArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            return OperationResult<byte[]>.Success(result.Value);
        }
        return OperationResult<byte[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<short>> ReadInt16Async(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length >= 2)
        {
            return OperationResult<short>.Success(BitConverter.ToInt16(result.Value, 0));
        }
        return OperationResult<short>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<short[]>> ReadInt16ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new short[length];
            for (int i = 0; i < length && i * 2 + 1 < result.Value.Length; i++)
            {
                values[i] = BitConverter.ToInt16(result.Value, i * 2);
            }
            return OperationResult<short[]>.Success(values);
        }
        return OperationResult<short[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<ushort>> ReadUInt16Async(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadInt16Async(address, cancellationToken);
        if (result.IsSuccess)
        {
            return OperationResult<ushort>.Success((ushort)result.Value);
        }
        return OperationResult<ushort>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<ushort[]>> ReadUInt16ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadInt16ArrayAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            return OperationResult<ushort[]>.Success(result.Value.Select(v => (ushort)v).ToArray());
        }
        return OperationResult<ushort[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<int>> ReadInt32Async(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length >= 4)
        {
            return OperationResult<int>.Success(BitConverter.ToInt32(result.Value, 0));
        }
        return OperationResult<int>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<int[]>> ReadInt32ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new int[length];
            for (int i = 0; i < length && i * 4 + 3 < result.Value.Length; i++)
            {
                values[i] = BitConverter.ToInt32(result.Value, i * 4);
            }
            return OperationResult<int[]>.Success(values);
        }
        return OperationResult<int[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<uint>> ReadUInt32Async(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadInt32Async(address, cancellationToken);
        if (result.IsSuccess)
        {
            return OperationResult<uint>.Success((uint)result.Value);
        }
        return OperationResult<uint>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<uint[]>> ReadUInt32ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadInt32ArrayAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            return OperationResult<uint[]>.Success(result.Value.Select(v => (uint)v).ToArray());
        }
        return OperationResult<uint[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<long>> ReadInt64Async(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length >= 8)
        {
            return OperationResult<long>.Success(BitConverter.ToInt64(result.Value, 0));
        }
        return OperationResult<long>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<long[]>> ReadInt64ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new long[length];
            for (int i = 0; i < length && i * 8 + 7 < result.Value.Length; i++)
            {
                values[i] = BitConverter.ToInt64(result.Value, i * 8);
            }
            return OperationResult<long[]>.Success(values);
        }
        return OperationResult<long[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<ulong>> ReadUInt64Async(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadInt64Async(address, cancellationToken);
        if (result.IsSuccess)
        {
            return OperationResult<ulong>.Success((ulong)result.Value);
        }
        return OperationResult<ulong>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<ulong[]>> ReadUInt64ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadInt64ArrayAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            return OperationResult<ulong[]>.Success(result.Value.Select(v => (ulong)v).ToArray());
        }
        return OperationResult<ulong[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<float>> ReadFloatAsync(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length >= 4)
        {
            return OperationResult<float>.Success(BitConverter.ToSingle(result.Value, 0));
        }
        return OperationResult<float>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<float[]>> ReadFloatArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new float[length];
            for (int i = 0; i < length && i * 4 + 3 < result.Value.Length; i++)
            {
                values[i] = BitConverter.ToSingle(result.Value, i * 4);
            }
            return OperationResult<float[]>.Success(values);
        }
        return OperationResult<float[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<double>> ReadDoubleAsync(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length >= 8)
        {
            return OperationResult<double>.Success(BitConverter.ToDouble(result.Value, 0));
        }
        return OperationResult<double>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<double[]>> ReadDoubleArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new double[length];
            for (int i = 0; i < length && i * 8 + 7 < result.Value.Length; i++)
            {
                values[i] = BitConverter.ToDouble(result.Value, i * 8);
            }
            return OperationResult<double[]>.Success(values);
        }
        return OperationResult<double[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<string>> ReadStringAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadTagAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            // AB strings have a 4-byte length prefix
            if (result.Value.Length >= 4)
            {
                var strLength = BitConverter.ToInt32(result.Value, 0);
                strLength = Math.Min(strLength, result.Value.Length - 4);
                var str = Encoding.ASCII.GetString(result.Value, 4, strLength).TrimEnd('\0');
                return OperationResult<string>.Success(str);
            }
            var str2 = Encoding.ASCII.GetString(result.Value).TrimEnd('\0');
            return OperationResult<string>.Success(str2);
        }
        return OperationResult<string>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    #endregion

    #region Write Operations

    public override async Task<OperationResult> WriteBoolAsync(string address, bool value, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, new byte[] { value ? (byte)1 : (byte)0 }, CipTypeBool, cancellationToken);
    }

    public override async Task<OperationResult> WriteBoolArrayAsync(string address, bool[] values, CancellationToken cancellationToken = default)
    {
        var bytes = values.Select(v => v ? (byte)1 : (byte)0).ToArray();
        return await WriteTagAsync(address, bytes, CipTypeBool, cancellationToken);
    }

    public override async Task<OperationResult> WriteByteAsync(string address, byte value, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, new byte[] { value }, CipTypeUsint, cancellationToken);
    }

    public override async Task<OperationResult> WriteByteArrayAsync(string address, byte[] values, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, values, CipTypeUsint, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt16Async(string address, short value, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, BitConverter.GetBytes(value), CipTypeInt, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt16ArrayAsync(string address, short[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 2];
        for (int i = 0; i < values.Length; i++)
        {
            BitConverter.GetBytes(values[i]).CopyTo(bytes, i * 2);
        }
        return await WriteTagAsync(address, bytes, CipTypeInt, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt16Async(string address, ushort value, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, BitConverter.GetBytes(value), CipTypeUint, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt16ArrayAsync(string address, ushort[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 2];
        for (int i = 0; i < values.Length; i++)
        {
            BitConverter.GetBytes(values[i]).CopyTo(bytes, i * 2);
        }
        return await WriteTagAsync(address, bytes, CipTypeUint, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt32Async(string address, int value, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, BitConverter.GetBytes(value), CipTypeDint, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt32ArrayAsync(string address, int[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 4];
        for (int i = 0; i < values.Length; i++)
        {
            BitConverter.GetBytes(values[i]).CopyTo(bytes, i * 4);
        }
        return await WriteTagAsync(address, bytes, CipTypeDint, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt32Async(string address, uint value, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, BitConverter.GetBytes(value), CipTypeUdint, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt32ArrayAsync(string address, uint[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 4];
        for (int i = 0; i < values.Length; i++)
        {
            BitConverter.GetBytes(values[i]).CopyTo(bytes, i * 4);
        }
        return await WriteTagAsync(address, bytes, CipTypeUdint, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt64Async(string address, long value, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, BitConverter.GetBytes(value), CipTypeLint, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt64ArrayAsync(string address, long[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 8];
        for (int i = 0; i < values.Length; i++)
        {
            BitConverter.GetBytes(values[i]).CopyTo(bytes, i * 8);
        }
        return await WriteTagAsync(address, bytes, CipTypeLint, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt64Async(string address, ulong value, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, BitConverter.GetBytes(value), CipTypeUlint, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt64ArrayAsync(string address, ulong[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 8];
        for (int i = 0; i < values.Length; i++)
        {
            BitConverter.GetBytes(values[i]).CopyTo(bytes, i * 8);
        }
        return await WriteTagAsync(address, bytes, CipTypeUlint, cancellationToken);
    }

    public override async Task<OperationResult> WriteFloatAsync(string address, float value, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, BitConverter.GetBytes(value), CipTypeReal, cancellationToken);
    }

    public override async Task<OperationResult> WriteFloatArrayAsync(string address, float[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 4];
        for (int i = 0; i < values.Length; i++)
        {
            BitConverter.GetBytes(values[i]).CopyTo(bytes, i * 4);
        }
        return await WriteTagAsync(address, bytes, CipTypeReal, cancellationToken);
    }

    public override async Task<OperationResult> WriteDoubleAsync(string address, double value, CancellationToken cancellationToken = default)
    {
        return await WriteTagAsync(address, BitConverter.GetBytes(value), CipTypeLreal, cancellationToken);
    }

    public override async Task<OperationResult> WriteDoubleArrayAsync(string address, double[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 8];
        for (int i = 0; i < values.Length; i++)
        {
            BitConverter.GetBytes(values[i]).CopyTo(bytes, i * 8);
        }
        return await WriteTagAsync(address, bytes, CipTypeLreal, cancellationToken);
    }

    public override async Task<OperationResult> WriteStringAsync(string address, string value, CancellationToken cancellationToken = default)
    {
        // AB strings have a 4-byte length prefix
        var strBytes = Encoding.ASCII.GetBytes(value);
        var bytes = new byte[4 + strBytes.Length];
        BitConverter.GetBytes(strBytes.Length).CopyTo(bytes, 0);
        strBytes.CopyTo(bytes, 4);
        return await WriteTagAsync(address, bytes, CipTypeString, cancellationToken);
    }

    #endregion
}
