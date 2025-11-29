using System.Text;
using Spider.Drivers.Communication.Interfaces;
using Spider.Drivers.Communication.Models;

namespace Spider.Drivers.Communication.Drivers.Siemens;

/// <summary>
/// Siemens S7 (S7-COMM) driver implementation
/// </summary>
public class SiemensS7Driver : PlcDriverBase
{
    private readonly SiemensS7Settings _s7Settings;
    private ushort _pduLength = 240;
    private ushort _incrementId;

    public override string DriverTypeName => "SiemensS7";

    public SiemensS7Driver(SiemensS7Settings settings) : base(settings)
    {
        _s7Settings = settings;
    }

    #region S7 Protocol Constants

    private static readonly byte[] COTP_CR = { 0x11, 0xE0, 0x00, 0x00, 0x00, 0x01, 0x00, 0xC0, 0x01, 0x0A, 0xC1, 0x02, 0x01, 0x02, 0xC2, 0x02, 0x01, 0x00 };
    private static readonly byte[] S7_SETUP_COMM = { 0xF0, 0x00, 0x00, 0x01, 0x00, 0x01, 0x01, 0xE0 };

    #endregion

    #region Connection Methods

    protected override async Task<OperationResult> InitializeConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Send COTP Connection Request
            var cotpResult = await SendCotpConnectionRequestAsync(cancellationToken);
            if (!cotpResult.IsSuccess)
            {
                return cotpResult;
            }

            // Send S7 Setup Communication
            var setupResult = await SendS7SetupCommunicationAsync(cancellationToken);
            if (!setupResult.IsSuccess)
            {
                return setupResult;
            }

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"S7 initialization failed: {ex.Message}");
        }
    }

    private async Task<OperationResult> SendCotpConnectionRequestAsync(CancellationToken cancellationToken)
    {
        // Build COTP Connection Request
        byte[] cotpCR = BuildCotpConnectionRequest();

        // TPKT Header
        var tpkt = new byte[4 + cotpCR.Length];
        tpkt[0] = 0x03; // Version
        tpkt[1] = 0x00;
        tpkt[2] = (byte)((tpkt.Length >> 8) & 0xFF);
        tpkt[3] = (byte)(tpkt.Length & 0xFF);
        Array.Copy(cotpCR, 0, tpkt, 4, cotpCR.Length);

        var response = await SendAndReceiveVariableLengthAsync(tpkt, GetTpktLength, 4, cancellationToken);

        if (response == null || response.Length < 7)
        {
            return OperationResult.Failure("COTP connection request failed - no response");
        }

        // Check COTP CC response (0xD0)
        if (response[5] != 0xD0)
        {
            return OperationResult.Failure($"COTP connection rejected: {response[5]:X2}");
        }

        return OperationResult.Success();
    }

    private byte[] BuildCotpConnectionRequest()
    {
        var cotpCR = new byte[COTP_CR.Length];
        Array.Copy(COTP_CR, cotpCR, COTP_CR.Length);

        // Set source and destination TSAP based on PLC type
        var rack = _s7Settings.Rack;
        var slot = _s7Settings.Slot;
        var connectionType = _s7Settings.ConnectionType;

        // Source TSAP
        cotpCR[11] = connectionType; // Usually 0x01 for PG, 0x02 for OP, 0x03 for Basic

        // Destination TSAP
        switch (_s7Settings.PlcType)
        {
            case SiemensPlcType.S200:
            case SiemensPlcType.S200Smart:
                cotpCR[15] = 0x10;
                break;
            case SiemensPlcType.S300:
            case SiemensPlcType.S400:
            case SiemensPlcType.S1200:
            case SiemensPlcType.S1500:
            default:
                cotpCR[15] = (byte)((rack << 5) | slot);
                break;
        }

        return cotpCR;
    }

    private async Task<OperationResult> SendS7SetupCommunicationAsync(CancellationToken cancellationToken)
    {
        // S7 Setup Communication
        var s7Setup = new byte[] {
            0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00,
            0xF0, 0x00, 0x00, 0x01, 0x00, 0x01, 0x01, 0xE0
        };

        // COTP Data header
        var cotp = new byte[] { 0x02, 0xF0, 0x80 };

        var tpkt = new byte[4 + cotp.Length + s7Setup.Length];
        tpkt[0] = 0x03;
        tpkt[1] = 0x00;
        tpkt[2] = (byte)((tpkt.Length >> 8) & 0xFF);
        tpkt[3] = (byte)(tpkt.Length & 0xFF);
        Array.Copy(cotp, 0, tpkt, 4, cotp.Length);
        Array.Copy(s7Setup, 0, tpkt, 4 + cotp.Length, s7Setup.Length);

        var response = await SendAndReceiveVariableLengthAsync(tpkt, GetTpktLength, 4, cancellationToken);

        if (response == null || response.Length < 20)
        {
            return OperationResult.Failure("S7 setup communication failed - no response");
        }

        // Extract negotiated PDU length
        if (response.Length >= 27)
        {
            _pduLength = (ushort)((response[25] << 8) | response[26]);
        }

        return OperationResult.Success();
    }

    private static int GetTpktLength(byte[] header, int headerLength)
    {
        if (headerLength >= 4)
        {
            return (header[2] << 8) | header[3];
        }
        return headerLength;
    }

    #endregion

    #region S7 Read/Write Methods

    private async Task<OperationResult<byte[]>> ReadBytesInternalAsync(string address, ushort length, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            return OperationResult<byte[]>.Failure("Not connected");
        }

        try
        {
            var addressResult = ParseS7Address(address);
            if (!addressResult.IsSuccess)
            {
                return OperationResult<byte[]>.Failure(addressResult.ErrorMessage!);
            }

            var (areaCode, dbNumber, startAddress, bitAddress, isBit) = addressResult.Value;

            // Build S7 Read Request
            var request = BuildS7ReadRequest(areaCode, dbNumber, startAddress, length, isBit, bitAddress);

            var response = await SendAndReceiveVariableLengthAsync(request, GetTpktLength, 4, cancellationToken);

            if (response == null)
            {
                return OperationResult<byte[]>.Failure("No response received");
            }

            return ParseS7ReadResponse(response, length, isBit);
        }
        catch (Exception ex)
        {
            return OperationResult<byte[]>.Failure($"Read failed: {ex.Message}");
        }
    }

    private async Task<OperationResult> WriteBytesInternalAsync(string address, byte[] data, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            return OperationResult.Failure("Not connected");
        }

        try
        {
            var addressResult = ParseS7Address(address);
            if (!addressResult.IsSuccess)
            {
                return OperationResult.Failure(addressResult.ErrorMessage!);
            }

            var (areaCode, dbNumber, startAddress, bitAddress, isBit) = addressResult.Value;

            // Build S7 Write Request
            var request = BuildS7WriteRequest(areaCode, dbNumber, startAddress, data, isBit, bitAddress);

            var response = await SendAndReceiveVariableLengthAsync(request, GetTpktLength, 4, cancellationToken);

            if (response == null)
            {
                return OperationResult.Failure("No response received");
            }

            return ParseS7WriteResponse(response);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Write failed: {ex.Message}");
        }
    }

    private byte[] BuildS7ReadRequest(byte areaCode, ushort dbNumber, int startAddress, ushort length, bool isBit, byte bitAddress)
    {
        var itemSize = isBit ? 1 : length;
        var transportSize = isBit ? (byte)0x01 : (byte)0x02; // 0x01 = BIT, 0x02 = BYTE

        // S7 COMM Read Request
        var s7Request = new byte[19];
        s7Request[0] = 0x32; // Protocol ID
        s7Request[1] = 0x01; // ROSCTR: Job
        s7Request[2] = 0x00;
        s7Request[3] = 0x00;
        s7Request[4] = (byte)((_incrementId >> 8) & 0xFF);
        s7Request[5] = (byte)(_incrementId++ & 0xFF);
        s7Request[6] = 0x00;
        s7Request[7] = 0x0E; // Parameter length
        s7Request[8] = 0x00;
        s7Request[9] = 0x00; // Data length
        s7Request[10] = 0x04; // Function: Read Var
        s7Request[11] = 0x01; // Item count

        // Address specification
        s7Request[12] = 0x12; // Variable specification
        s7Request[13] = 0x0A; // Length of remaining address specification
        s7Request[14] = 0x10; // Syntax ID: S7ANY
        s7Request[15] = transportSize;
        s7Request[16] = (byte)((itemSize >> 8) & 0xFF);
        s7Request[17] = (byte)(itemSize & 0xFF);
        s7Request[18] = (byte)((dbNumber >> 8) & 0xFF);

        // Extend to include full address
        var fullRequest = new byte[31];
        Array.Copy(s7Request, 0, fullRequest, 0, 19);
        fullRequest[19] = (byte)(dbNumber & 0xFF);
        fullRequest[20] = areaCode;

        // Start address (3 bytes: byte.byte.bit)
        var byteAddr = isBit ? (startAddress * 8 + bitAddress) : (startAddress * 8);
        fullRequest[21] = (byte)((byteAddr >> 16) & 0xFF);
        fullRequest[22] = (byte)((byteAddr >> 8) & 0xFF);
        fullRequest[23] = (byte)(byteAddr & 0xFF);

        // COTP Data
        var cotp = new byte[] { 0x02, 0xF0, 0x80 };

        // Build TPKT + COTP + S7
        var tpkt = new byte[4 + cotp.Length + 24];
        tpkt[0] = 0x03;
        tpkt[1] = 0x00;
        tpkt[2] = (byte)((tpkt.Length >> 8) & 0xFF);
        tpkt[3] = (byte)(tpkt.Length & 0xFF);
        Array.Copy(cotp, 0, tpkt, 4, cotp.Length);
        Array.Copy(fullRequest, 0, tpkt, 4 + cotp.Length, 24);

        return tpkt;
    }

    private byte[] BuildS7WriteRequest(byte areaCode, ushort dbNumber, int startAddress, byte[] data, bool isBit, byte bitAddress)
    {
        var dataLen = data.Length;
        var transportSize = isBit ? (byte)0x03 : (byte)0x04; // 0x03 = BIT, 0x04 = BYTE
        var dataBitLength = isBit ? dataLen : dataLen * 8;

        // Calculate lengths
        var paramLength = 14;
        var dataLength = 4 + dataLen + (dataLen % 2); // Padding to even

        // S7 COMM Write Request
        var s7Request = new byte[10 + paramLength + dataLength];
        s7Request[0] = 0x32; // Protocol ID
        s7Request[1] = 0x01; // ROSCTR: Job
        s7Request[2] = 0x00;
        s7Request[3] = 0x00;
        s7Request[4] = (byte)((_incrementId >> 8) & 0xFF);
        s7Request[5] = (byte)(_incrementId++ & 0xFF);
        s7Request[6] = (byte)((paramLength >> 8) & 0xFF);
        s7Request[7] = (byte)(paramLength & 0xFF);
        s7Request[8] = (byte)((dataLength >> 8) & 0xFF);
        s7Request[9] = (byte)(dataLength & 0xFF);
        s7Request[10] = 0x05; // Function: Write Var
        s7Request[11] = 0x01; // Item count

        // Address specification
        s7Request[12] = 0x12; // Variable specification
        s7Request[13] = 0x0A; // Length
        s7Request[14] = 0x10; // Syntax ID: S7ANY
        s7Request[15] = isBit ? (byte)0x01 : (byte)0x02; // Transport size for address
        s7Request[16] = (byte)((dataLen >> 8) & 0xFF);
        s7Request[17] = (byte)(dataLen & 0xFF);
        s7Request[18] = (byte)((dbNumber >> 8) & 0xFF);
        s7Request[19] = (byte)(dbNumber & 0xFF);
        s7Request[20] = areaCode;

        // Start address
        var byteAddr = isBit ? (startAddress * 8 + bitAddress) : (startAddress * 8);
        s7Request[21] = (byte)((byteAddr >> 16) & 0xFF);
        s7Request[22] = (byte)((byteAddr >> 8) & 0xFF);
        s7Request[23] = (byte)(byteAddr & 0xFF);

        // Data header
        s7Request[24] = 0x00; // Return code
        s7Request[25] = transportSize;
        s7Request[26] = (byte)((dataBitLength >> 8) & 0xFF);
        s7Request[27] = (byte)(dataBitLength & 0xFF);

        // Data
        Array.Copy(data, 0, s7Request, 28, dataLen);

        // COTP Data
        var cotp = new byte[] { 0x02, 0xF0, 0x80 };

        // Build TPKT + COTP + S7
        var tpkt = new byte[4 + cotp.Length + s7Request.Length];
        tpkt[0] = 0x03;
        tpkt[1] = 0x00;
        tpkt[2] = (byte)((tpkt.Length >> 8) & 0xFF);
        tpkt[3] = (byte)(tpkt.Length & 0xFF);
        Array.Copy(cotp, 0, tpkt, 4, cotp.Length);
        Array.Copy(s7Request, 0, tpkt, 4 + cotp.Length, s7Request.Length);

        return tpkt;
    }

    private static OperationResult<byte[]> ParseS7ReadResponse(byte[] response, ushort expectedLength, bool isBit)
    {
        // Minimum response: TPKT(4) + COTP(3) + S7 Header(12) + Return Code(1)
        if (response.Length < 20)
        {
            return OperationResult<byte[]>.Failure("Response too short");
        }

        // Check S7 response header
        var s7Offset = 7; // After TPKT + COTP
        if (response[s7Offset] != 0x32)
        {
            return OperationResult<byte[]>.Failure("Invalid S7 response");
        }

        // Check for errors
        var errorClass = response[s7Offset + 10];
        var errorCode = response[s7Offset + 11];

        if (errorClass != 0 || errorCode != 0)
        {
            return OperationResult<byte[]>.Failure($"S7 error: class={errorClass}, code={errorCode}", (errorClass << 8) | errorCode);
        }

        // Get data
        var paramLength = (response[s7Offset + 6] << 8) | response[s7Offset + 7];
        var dataOffset = s7Offset + 12 + paramLength;

        if (dataOffset >= response.Length)
        {
            return OperationResult<byte[]>.Failure("Invalid response format");
        }

        // Check return code
        var returnCode = response[dataOffset];
        if (returnCode != 0xFF)
        {
            return OperationResult<byte[]>.Failure($"Read failed with return code: {returnCode}", returnCode);
        }

        // Extract data
        var dataLength = (response[dataOffset + 2] << 8) | response[dataOffset + 3];
        if (!isBit)
        {
            dataLength /= 8; // Convert bits to bytes
        }

        var data = new byte[dataLength];
        Array.Copy(response, dataOffset + 4, data, 0, Math.Min(dataLength, response.Length - dataOffset - 4));

        return OperationResult<byte[]>.Success(data);
    }

    private static OperationResult ParseS7WriteResponse(byte[] response)
    {
        if (response.Length < 20)
        {
            return OperationResult.Failure("Response too short");
        }

        var s7Offset = 7;
        if (response[s7Offset] != 0x32)
        {
            return OperationResult.Failure("Invalid S7 response");
        }

        var errorClass = response[s7Offset + 10];
        var errorCode = response[s7Offset + 11];

        if (errorClass != 0 || errorCode != 0)
        {
            return OperationResult.Failure($"S7 error: class={errorClass}, code={errorCode}", (errorClass << 8) | errorCode);
        }

        // Check write confirmation
        var paramLength = (response[s7Offset + 6] << 8) | response[s7Offset + 7];
        var dataOffset = s7Offset + 12 + paramLength;

        if (dataOffset < response.Length && response[dataOffset] != 0xFF)
        {
            return OperationResult.Failure($"Write failed with return code: {response[dataOffset]}", response[dataOffset]);
        }

        return OperationResult.Success();
    }

    private static OperationResult<(byte areaCode, ushort dbNumber, int startAddress, byte bitAddress, bool isBit)> ParseS7Address(string address)
    {
        // Supported formats:
        // DB10.DBX0.0 - Bit 0 of byte 0 in DB10
        // DB10.DBB0 - Byte 0 in DB10
        // DB10.DBW0 - Word at byte 0 in DB10
        // DB10.DBD0 - Double word at byte 0 in DB10
        // M0.0, I0.0, Q0.0 - Memory bit addresses
        // MB0, IB0, QB0 - Memory byte addresses
        // MW0, IW0, QW0 - Memory word addresses
        // MD0, ID0, QD0 - Memory double word addresses

        address = address.ToUpperInvariant();
        byte areaCode;
        ushort dbNumber = 0;
        int startAddress;
        byte bitAddress = 0;
        bool isBit = false;

        if (address.StartsWith("DB"))
        {
            // Data Block addressing
            areaCode = 0x84; // DB area

            var parts = address.Split('.');
            if (parts.Length < 2)
            {
                return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid DB address format: {address}");
            }

            if (!ushort.TryParse(parts[0][2..], out dbNumber))
            {
                return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid DB number: {parts[0]}");
            }

            var typeAndAddress = parts[1];
            if (typeAndAddress.StartsWith("DBX") && parts.Length >= 3)
            {
                // Bit address: DB10.DBX0.1
                isBit = true;
                if (!int.TryParse(typeAndAddress[3..], out startAddress) || !byte.TryParse(parts[2], out bitAddress))
                {
                    return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid bit address: {address}");
                }
            }
            else if (typeAndAddress.StartsWith("DBB"))
            {
                if (!int.TryParse(typeAndAddress[3..], out startAddress))
                {
                    return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid byte address: {address}");
                }
            }
            else if (typeAndAddress.StartsWith("DBW"))
            {
                if (!int.TryParse(typeAndAddress[3..], out startAddress))
                {
                    return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid word address: {address}");
                }
            }
            else if (typeAndAddress.StartsWith("DBD"))
            {
                if (!int.TryParse(typeAndAddress[3..], out startAddress))
                {
                    return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid dword address: {address}");
                }
            }
            else
            {
                return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid DB element type: {typeAndAddress}");
            }
        }
        else if (address.StartsWith("M") || address.StartsWith("I") || address.StartsWith("Q"))
        {
            // Memory area addressing
            areaCode = address[0] switch
            {
                'M' => 0x83, // Memory
                'I' => 0x81, // Input
                'Q' => 0x82, // Output
                _ => throw new ArgumentException($"Invalid area: {address[0]}")
            };

            var rest = address[1..];

            if (rest.Contains('.'))
            {
                // Bit address: M0.1
                isBit = true;
                var parts = rest.Split('.');
                if (!int.TryParse(parts[0], out startAddress) || !byte.TryParse(parts[1], out bitAddress))
                {
                    return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid memory bit address: {address}");
                }
            }
            else if (rest.StartsWith("B"))
            {
                if (!int.TryParse(rest[1..], out startAddress))
                {
                    return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid memory byte address: {address}");
                }
            }
            else if (rest.StartsWith("W"))
            {
                if (!int.TryParse(rest[1..], out startAddress))
                {
                    return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid memory word address: {address}");
                }
            }
            else if (rest.StartsWith("D"))
            {
                if (!int.TryParse(rest[1..], out startAddress))
                {
                    return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid memory dword address: {address}");
                }
            }
            else
            {
                // Try as bit address without dot (M0)
                isBit = true;
                if (!int.TryParse(rest, out startAddress))
                {
                    return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Invalid memory address: {address}");
                }
            }
        }
        else
        {
            return OperationResult<(byte, ushort, int, byte, bool)>.Failure($"Unsupported address format: {address}");
        }

        return OperationResult<(byte, ushort, int, byte, bool)>.Success((areaCode, dbNumber, startAddress, bitAddress, isBit));
    }

    #endregion

    #region Read Operations

    public override async Task<OperationResult<bool>> ReadBoolAsync(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadBytesInternalAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value != null && result.Value.Length > 0)
        {
            return OperationResult<bool>.Success(result.Value[0] != 0);
        }
        return OperationResult<bool>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<bool[]>> ReadBoolArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var byteLength = (ushort)((length + 7) / 8);
        var result = await ReadBytesInternalAsync(address, byteLength, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var bools = new bool[length];
            for (int i = 0; i < length; i++)
            {
                bools[i] = ((result.Value[i / 8] >> (i % 8)) & 1) != 0;
            }
            return OperationResult<bool[]>.Success(bools);
        }
        return OperationResult<bool[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<byte>> ReadByteAsync(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadBytesInternalAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length > 0)
        {
            return OperationResult<byte>.Success(result.Value[0]);
        }
        return OperationResult<byte>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<byte[]>> ReadByteArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        return await ReadBytesInternalAsync(address, length, cancellationToken);
    }

    public override async Task<OperationResult<short>> ReadInt16Async(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadBytesInternalAsync(address, 2, cancellationToken);
        if (result.IsSuccess && result.Value?.Length >= 2)
        {
            return OperationResult<short>.Success((short)((result.Value[0] << 8) | result.Value[1]));
        }
        return OperationResult<short>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<short[]>> ReadInt16ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadBytesInternalAsync(address, (ushort)(length * 2), cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new short[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = (short)((result.Value[i * 2] << 8) | result.Value[i * 2 + 1]);
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
        var result = await ReadBytesInternalAsync(address, 4, cancellationToken);
        if (result.IsSuccess && result.Value?.Length >= 4)
        {
            return OperationResult<int>.Success((result.Value[0] << 24) | (result.Value[1] << 16) | (result.Value[2] << 8) | result.Value[3]);
        }
        return OperationResult<int>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<int[]>> ReadInt32ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadBytesInternalAsync(address, (ushort)(length * 4), cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new int[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = (result.Value[i * 4] << 24) | (result.Value[i * 4 + 1] << 16) | 
                           (result.Value[i * 4 + 2] << 8) | result.Value[i * 4 + 3];
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
        var result = await ReadBytesInternalAsync(address, 8, cancellationToken);
        if (result.IsSuccess && result.Value?.Length >= 8)
        {
            return OperationResult<long>.Success(
                ((long)result.Value[0] << 56) | ((long)result.Value[1] << 48) |
                ((long)result.Value[2] << 40) | ((long)result.Value[3] << 32) |
                ((long)result.Value[4] << 24) | ((long)result.Value[5] << 16) |
                ((long)result.Value[6] << 8) | result.Value[7]);
        }
        return OperationResult<long>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<long[]>> ReadInt64ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadBytesInternalAsync(address, (ushort)(length * 8), cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new long[length];
            for (int i = 0; i < length; i++)
            {
                var offset = i * 8;
                values[i] = ((long)result.Value[offset] << 56) | ((long)result.Value[offset + 1] << 48) |
                           ((long)result.Value[offset + 2] << 40) | ((long)result.Value[offset + 3] << 32) |
                           ((long)result.Value[offset + 4] << 24) | ((long)result.Value[offset + 5] << 16) |
                           ((long)result.Value[offset + 6] << 8) | result.Value[offset + 7];
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
        var result = await ReadBytesInternalAsync(address, 4, cancellationToken);
        if (result.IsSuccess && result.Value?.Length >= 4)
        {
            // Siemens uses big-endian for floats
            var bytes = new byte[] { result.Value[3], result.Value[2], result.Value[1], result.Value[0] };
            return OperationResult<float>.Success(BitConverter.ToSingle(bytes, 0));
        }
        return OperationResult<float>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<float[]>> ReadFloatArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadBytesInternalAsync(address, (ushort)(length * 4), cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new float[length];
            for (int i = 0; i < length; i++)
            {
                var offset = i * 4;
                var bytes = new byte[] { result.Value[offset + 3], result.Value[offset + 2], 
                                        result.Value[offset + 1], result.Value[offset] };
                values[i] = BitConverter.ToSingle(bytes, 0);
            }
            return OperationResult<float[]>.Success(values);
        }
        return OperationResult<float[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<double>> ReadDoubleAsync(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadBytesInternalAsync(address, 8, cancellationToken);
        if (result.IsSuccess && result.Value?.Length >= 8)
        {
            var bytes = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                bytes[7 - i] = result.Value[i];
            }
            return OperationResult<double>.Success(BitConverter.ToDouble(bytes, 0));
        }
        return OperationResult<double>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<double[]>> ReadDoubleArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadBytesInternalAsync(address, (ushort)(length * 8), cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new double[length];
            for (int i = 0; i < length; i++)
            {
                var offset = i * 8;
                var bytes = new byte[8];
                for (int j = 0; j < 8; j++)
                {
                    bytes[7 - j] = result.Value[offset + j];
                }
                values[i] = BitConverter.ToDouble(bytes, 0);
            }
            return OperationResult<double[]>.Success(values);
        }
        return OperationResult<double[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<string>> ReadStringAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadBytesInternalAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var str = Encoding.ASCII.GetString(result.Value).TrimEnd('\0');
            return OperationResult<string>.Success(str);
        }
        return OperationResult<string>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    #endregion

    #region Write Operations

    public override async Task<OperationResult> WriteBoolAsync(string address, bool value, CancellationToken cancellationToken = default)
    {
        return await WriteBytesInternalAsync(address, new byte[] { value ? (byte)1 : (byte)0 }, cancellationToken);
    }

    public override async Task<OperationResult> WriteBoolArrayAsync(string address, bool[] values, CancellationToken cancellationToken = default)
    {
        var byteCount = (values.Length + 7) / 8;
        var bytes = new byte[byteCount];
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i])
            {
                bytes[i / 8] |= (byte)(1 << (i % 8));
            }
        }
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override async Task<OperationResult> WriteByteAsync(string address, byte value, CancellationToken cancellationToken = default)
    {
        return await WriteBytesInternalAsync(address, new byte[] { value }, cancellationToken);
    }

    public override async Task<OperationResult> WriteByteArrayAsync(string address, byte[] values, CancellationToken cancellationToken = default)
    {
        return await WriteBytesInternalAsync(address, values, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt16Async(string address, short value, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[] { (byte)(value >> 8), (byte)value };
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt16ArrayAsync(string address, short[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 2];
        for (int i = 0; i < values.Length; i++)
        {
            bytes[i * 2] = (byte)(values[i] >> 8);
            bytes[i * 2 + 1] = (byte)values[i];
        }
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override Task<OperationResult> WriteUInt16Async(string address, ushort value, CancellationToken cancellationToken = default)
    {
        return WriteInt16Async(address, (short)value, cancellationToken);
    }

    public override Task<OperationResult> WriteUInt16ArrayAsync(string address, ushort[] values, CancellationToken cancellationToken = default)
    {
        return WriteInt16ArrayAsync(address, values.Select(v => (short)v).ToArray(), cancellationToken);
    }

    public override async Task<OperationResult> WriteInt32Async(string address, int value, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt32ArrayAsync(string address, int[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 4];
        for (int i = 0; i < values.Length; i++)
        {
            bytes[i * 4] = (byte)(values[i] >> 24);
            bytes[i * 4 + 1] = (byte)(values[i] >> 16);
            bytes[i * 4 + 2] = (byte)(values[i] >> 8);
            bytes[i * 4 + 3] = (byte)values[i];
        }
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override Task<OperationResult> WriteUInt32Async(string address, uint value, CancellationToken cancellationToken = default)
    {
        return WriteInt32Async(address, (int)value, cancellationToken);
    }

    public override Task<OperationResult> WriteUInt32ArrayAsync(string address, uint[] values, CancellationToken cancellationToken = default)
    {
        return WriteInt32ArrayAsync(address, values.Select(v => (int)v).ToArray(), cancellationToken);
    }

    public override async Task<OperationResult> WriteInt64Async(string address, long value, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            bytes[i] = (byte)(value >> (56 - i * 8));
        }
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt64ArrayAsync(string address, long[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 8];
        for (int i = 0; i < values.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                bytes[i * 8 + j] = (byte)(values[i] >> (56 - j * 8));
            }
        }
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override Task<OperationResult> WriteUInt64Async(string address, ulong value, CancellationToken cancellationToken = default)
    {
        return WriteInt64Async(address, (long)value, cancellationToken);
    }

    public override Task<OperationResult> WriteUInt64ArrayAsync(string address, ulong[] values, CancellationToken cancellationToken = default)
    {
        return WriteInt64ArrayAsync(address, values.Select(v => (long)v).ToArray(), cancellationToken);
    }

    public override async Task<OperationResult> WriteFloatAsync(string address, float value, CancellationToken cancellationToken = default)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes); // Convert to big-endian
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override async Task<OperationResult> WriteFloatArrayAsync(string address, float[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 4];
        for (int i = 0; i < values.Length; i++)
        {
            var valueBytes = BitConverter.GetBytes(values[i]);
            bytes[i * 4] = valueBytes[3];
            bytes[i * 4 + 1] = valueBytes[2];
            bytes[i * 4 + 2] = valueBytes[1];
            bytes[i * 4 + 3] = valueBytes[0];
        }
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override async Task<OperationResult> WriteDoubleAsync(string address, double value, CancellationToken cancellationToken = default)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override async Task<OperationResult> WriteDoubleArrayAsync(string address, double[] values, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[values.Length * 8];
        for (int i = 0; i < values.Length; i++)
        {
            var valueBytes = BitConverter.GetBytes(values[i]);
            for (int j = 0; j < 8; j++)
            {
                bytes[i * 8 + j] = valueBytes[7 - j];
            }
        }
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    public override async Task<OperationResult> WriteStringAsync(string address, string value, CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        return await WriteBytesInternalAsync(address, bytes, cancellationToken);
    }

    #endregion
}
