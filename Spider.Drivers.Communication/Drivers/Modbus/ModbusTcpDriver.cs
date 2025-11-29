using System.Text;
using Spider.Drivers.Communication.Interfaces;
using Spider.Drivers.Communication.Models;

namespace Spider.Drivers.Communication.Drivers.Modbus;

/// <summary>
/// Modbus TCP driver implementation
/// </summary>
public class ModbusTcpDriver : PlcDriverBase
{
    private readonly ModbusTcpSettings _modbusSettings;
    private ushort _transactionId;

    public override string DriverTypeName => "ModbusTCP";

    public ModbusTcpDriver(ModbusTcpSettings settings) : base(settings)
    {
        _modbusSettings = settings;
    }

    #region Modbus Protocol Constants

    private const byte FunctionReadCoils = 0x01;
    private const byte FunctionReadDiscreteInputs = 0x02;
    private const byte FunctionReadHoldingRegisters = 0x03;
    private const byte FunctionReadInputRegisters = 0x04;
    private const byte FunctionWriteSingleCoil = 0x05;
    private const byte FunctionWriteSingleRegister = 0x06;
    private const byte FunctionWriteMultipleCoils = 0x0F;
    private const byte FunctionWriteMultipleRegisters = 0x10;

    #endregion

    #region Private Helpers

    private byte[] BuildModbusRequest(byte function, ushort startAddress, ushort quantity, byte[]? dataBytes = null)
    {
        var transactionId = _transactionId++;
        var unitId = _modbusSettings.SlaveId;

        int pduLength = 6; // Unit ID + Function + Address (2) + Quantity (2)
        if (dataBytes != null)
        {
            pduLength += 1 + dataBytes.Length; // Byte count + data
        }

        var request = new byte[6 + pduLength]; // MBAP header (6) + PDU
        
        // MBAP Header
        request[0] = (byte)(transactionId >> 8);
        request[1] = (byte)transactionId;
        request[2] = 0; // Protocol ID high
        request[3] = 0; // Protocol ID low
        request[4] = (byte)((pduLength) >> 8);
        request[5] = (byte)(pduLength);
        
        // PDU
        request[6] = unitId;
        request[7] = function;
        request[8] = (byte)(startAddress >> 8);
        request[9] = (byte)startAddress;
        request[10] = (byte)(quantity >> 8);
        request[11] = (byte)quantity;

        if (dataBytes != null)
        {
            request[12] = (byte)dataBytes.Length;
            Array.Copy(dataBytes, 0, request, 13, dataBytes.Length);
        }

        return request;
    }

    private static int GetModbusResponseLength(byte[] header, int headerLength)
    {
        // MBAP header is 7 bytes, length field is at bytes 4-5
        if (headerLength >= 6)
        {
            return 6 + ((header[4] << 8) | header[5]);
        }
        return headerLength;
    }

    private async Task<OperationResult<ushort[]>> ReadRegistersAsync(byte function, ushort address, ushort count, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            return OperationResult<ushort[]>.Failure("Not connected");
        }

        try
        {
            var request = BuildModbusRequest(function, address, count);
            var response = await SendAndReceiveVariableLengthAsync(request, GetModbusResponseLength, 6, cancellationToken);

            if (response == null || response.Length < 9)
            {
                return OperationResult<ushort[]>.Failure("Invalid response");
            }

            // Check for Modbus exception
            if ((response[7] & 0x80) != 0)
            {
                return OperationResult<ushort[]>.Failure($"Modbus exception: {response[8]}", response[8]);
            }

            var byteCount = response[8];
            var registers = new ushort[count];

            for (int i = 0; i < count; i++)
            {
                registers[i] = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);
            }

            return OperationResult<ushort[]>.Success(registers);
        }
        catch (Exception ex)
        {
            return OperationResult<ushort[]>.Failure($"Read failed: {ex.Message}");
        }
    }

    private async Task<OperationResult<bool[]>> ReadBitsAsync(byte function, ushort address, ushort count, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            return OperationResult<bool[]>.Failure("Not connected");
        }

        try
        {
            var request = BuildModbusRequest(function, address, count);
            var response = await SendAndReceiveVariableLengthAsync(request, GetModbusResponseLength, 6, cancellationToken);

            if (response == null || response.Length < 9)
            {
                return OperationResult<bool[]>.Failure("Invalid response");
            }

            // Check for Modbus exception
            if ((response[7] & 0x80) != 0)
            {
                return OperationResult<bool[]>.Failure($"Modbus exception: {response[8]}", response[8]);
            }

            var byteCount = response[8];
            var bits = new bool[count];

            for (int i = 0; i < count; i++)
            {
                var byteIndex = i / 8;
                var bitIndex = i % 8;
                bits[i] = ((response[9 + byteIndex] >> bitIndex) & 0x01) == 1;
            }

            return OperationResult<bool[]>.Success(bits);
        }
        catch (Exception ex)
        {
            return OperationResult<bool[]>.Failure($"Read failed: {ex.Message}");
        }
    }

    private async Task<OperationResult> WriteRegistersAsync(ushort address, ushort[] values, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            return OperationResult.Failure("Not connected");
        }

        try
        {
            var dataBytes = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                dataBytes[i * 2] = (byte)(values[i] >> 8);
                dataBytes[i * 2 + 1] = (byte)values[i];
            }

            var request = BuildModbusRequest(FunctionWriteMultipleRegisters, address, (ushort)values.Length, dataBytes);
            var response = await SendAndReceiveVariableLengthAsync(request, GetModbusResponseLength, 6, cancellationToken);

            if (response == null || response.Length < 8)
            {
                return OperationResult.Failure("Invalid response");
            }

            // Check for Modbus exception
            if ((response[7] & 0x80) != 0)
            {
                return OperationResult.Failure($"Modbus exception: {response[8]}", response[8]);
            }

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Write failed: {ex.Message}");
        }
    }

    private async Task<OperationResult> WriteSingleRegisterAsync(ushort address, ushort value, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            return OperationResult.Failure("Not connected");
        }

        try
        {
            var request = BuildModbusRequest(FunctionWriteSingleRegister, address, value);
            var response = await SendAndReceiveAsync(request, 12, cancellationToken);

            if (response == null || response.Length < 8)
            {
                return OperationResult.Failure("Invalid response");
            }

            // Check for Modbus exception
            if ((response[7] & 0x80) != 0)
            {
                return OperationResult.Failure($"Modbus exception: {response[8]}", response[8]);
            }

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Write failed: {ex.Message}");
        }
    }

    private async Task<OperationResult> WriteSingleCoilAsync(ushort address, bool value, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            return OperationResult.Failure("Not connected");
        }

        try
        {
            var coilValue = value ? (ushort)0xFF00 : (ushort)0x0000;
            var request = BuildModbusRequest(FunctionWriteSingleCoil, address, coilValue);
            var response = await SendAndReceiveAsync(request, 12, cancellationToken);

            if (response == null || response.Length < 8)
            {
                return OperationResult.Failure("Invalid response");
            }

            // Check for Modbus exception
            if ((response[7] & 0x80) != 0)
            {
                return OperationResult.Failure($"Modbus exception: {response[8]}", response[8]);
            }

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Write failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Parse Modbus address format (e.g., "40001" for holding register 0, "00001" for coil 0)
    /// </summary>
    private static (byte function, ushort address) ParseModbusAddress(string address, bool isWrite = false)
    {
        // Handle format like "HR:0" (Holding Register 0) or "40001"
        if (address.StartsWith("HR:", StringComparison.OrdinalIgnoreCase) || 
            address.StartsWith("hr:", StringComparison.OrdinalIgnoreCase))
        {
            if (!ushort.TryParse(address.Substring(3), out var hrAddr))
            {
                throw new ArgumentException($"Invalid holding register address: {address}");
            }
            return (isWrite ? FunctionWriteMultipleRegisters : FunctionReadHoldingRegisters, hrAddr);
        }
        if (address.StartsWith("IR:", StringComparison.OrdinalIgnoreCase) ||
            address.StartsWith("ir:", StringComparison.OrdinalIgnoreCase))
        {
            if (!ushort.TryParse(address.Substring(3), out var irAddr))
            {
                throw new ArgumentException($"Invalid input register address: {address}");
            }
            return (FunctionReadInputRegisters, irAddr);
        }
        if (address.StartsWith("CS:", StringComparison.OrdinalIgnoreCase) ||
            address.StartsWith("cs:", StringComparison.OrdinalIgnoreCase))
        {
            if (!ushort.TryParse(address.Substring(3), out var csAddr))
            {
                throw new ArgumentException($"Invalid coil address: {address}");
            }
            return (isWrite ? FunctionWriteSingleCoil : FunctionReadCoils, csAddr);
        }
        if (address.StartsWith("IS:", StringComparison.OrdinalIgnoreCase) ||
            address.StartsWith("is:", StringComparison.OrdinalIgnoreCase))
        {
            if (!ushort.TryParse(address.Substring(3), out var isAddr))
            {
                throw new ArgumentException($"Invalid discrete input address: {address}");
            }
            return (FunctionReadDiscreteInputs, isAddr);
        }

        // Standard Modbus address format
        if (int.TryParse(address, out var numericAddress))
        {
            if (numericAddress >= 40001 && numericAddress <= 49999)
            {
                return (isWrite ? FunctionWriteMultipleRegisters : FunctionReadHoldingRegisters, 
                        (ushort)(numericAddress - 40001));
            }
            if (numericAddress >= 30001 && numericAddress <= 39999)
            {
                return (FunctionReadInputRegisters, (ushort)(numericAddress - 30001));
            }
            if (numericAddress >= 10001 && numericAddress <= 19999)
            {
                return (FunctionReadDiscreteInputs, (ushort)(numericAddress - 10001));
            }
            if (numericAddress >= 1 && numericAddress <= 9999)
            {
                return (isWrite ? FunctionWriteSingleCoil : FunctionReadCoils, (ushort)(numericAddress - 1));
            }
        }

        throw new ArgumentException($"Invalid Modbus address: {address}");
    }

    #endregion

    #region Read Operations

    public override async Task<OperationResult<bool>> ReadBoolAsync(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadBoolArrayAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length > 0)
        {
            return OperationResult<bool>.Success(result.Value[0]);
        }
        return OperationResult<bool>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<bool[]>> ReadBoolArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var (function, addr) = ParseModbusAddress(address);
        return await ReadBitsAsync(function, addr, length, cancellationToken);
    }

    public override async Task<OperationResult<byte>> ReadByteAsync(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadUInt16Async(address, cancellationToken);
        if (result.IsSuccess)
        {
            return OperationResult<byte>.Success((byte)result.Value);
        }
        return OperationResult<byte>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<byte[]>> ReadByteArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var registerCount = (ushort)((length + 1) / 2);
        var result = await ReadUInt16ArrayAsync(address, registerCount, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var bytes = new byte[length];
            for (int i = 0; i < registerCount && i * 2 < length; i++)
            {
                bytes[i * 2] = (byte)(result.Value[i] >> 8);
                if (i * 2 + 1 < length)
                {
                    bytes[i * 2 + 1] = (byte)result.Value[i];
                }
            }
            return OperationResult<byte[]>.Success(bytes);
        }
        return OperationResult<byte[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<short>> ReadInt16Async(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadUInt16Async(address, cancellationToken);
        if (result.IsSuccess)
        {
            return OperationResult<short>.Success((short)result.Value);
        }
        return OperationResult<short>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<short[]>> ReadInt16ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadUInt16ArrayAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            return OperationResult<short[]>.Success(result.Value.Select(v => (short)v).ToArray());
        }
        return OperationResult<short[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<ushort>> ReadUInt16Async(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadUInt16ArrayAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length > 0)
        {
            return OperationResult<ushort>.Success(result.Value[0]);
        }
        return OperationResult<ushort>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<ushort[]>> ReadUInt16ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var (function, addr) = ParseModbusAddress(address);
        return await ReadRegistersAsync(function, addr, length, cancellationToken);
    }

    public override async Task<OperationResult<int>> ReadInt32Async(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadInt32ArrayAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length > 0)
        {
            return OperationResult<int>.Success(result.Value[0]);
        }
        return OperationResult<int>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<int[]>> ReadInt32ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadUInt16ArrayAsync(address, (ushort)(length * 2), cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new int[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = (result.Value[i * 2] << 16) | result.Value[i * 2 + 1];
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
        var result = await ReadInt64ArrayAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length > 0)
        {
            return OperationResult<long>.Success(result.Value[0]);
        }
        return OperationResult<long>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<long[]>> ReadInt64ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadUInt16ArrayAsync(address, (ushort)(length * 4), cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new long[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = ((long)result.Value[i * 4] << 48) |
                           ((long)result.Value[i * 4 + 1] << 32) |
                           ((long)result.Value[i * 4 + 2] << 16) |
                           result.Value[i * 4 + 3];
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
        var result = await ReadFloatArrayAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length > 0)
        {
            return OperationResult<float>.Success(result.Value[0]);
        }
        return OperationResult<float>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<float[]>> ReadFloatArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadUInt16ArrayAsync(address, (ushort)(length * 2), cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new float[length];
            for (int i = 0; i < length; i++)
            {
                var bytes = new byte[4];
                bytes[3] = (byte)(result.Value[i * 2] >> 8);
                bytes[2] = (byte)result.Value[i * 2];
                bytes[1] = (byte)(result.Value[i * 2 + 1] >> 8);
                bytes[0] = (byte)result.Value[i * 2 + 1];
                values[i] = BitConverter.ToSingle(bytes, 0);
            }
            return OperationResult<float[]>.Success(values);
        }
        return OperationResult<float[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<double>> ReadDoubleAsync(string address, CancellationToken cancellationToken = default)
    {
        var result = await ReadDoubleArrayAsync(address, 1, cancellationToken);
        if (result.IsSuccess && result.Value?.Length > 0)
        {
            return OperationResult<double>.Success(result.Value[0]);
        }
        return OperationResult<double>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<double[]>> ReadDoubleArrayAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadUInt16ArrayAsync(address, (ushort)(length * 4), cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var values = new double[length];
            for (int i = 0; i < length; i++)
            {
                var bytes = new byte[8];
                bytes[7] = (byte)(result.Value[i * 4] >> 8);
                bytes[6] = (byte)result.Value[i * 4];
                bytes[5] = (byte)(result.Value[i * 4 + 1] >> 8);
                bytes[4] = (byte)result.Value[i * 4 + 1];
                bytes[3] = (byte)(result.Value[i * 4 + 2] >> 8);
                bytes[2] = (byte)result.Value[i * 4 + 2];
                bytes[1] = (byte)(result.Value[i * 4 + 3] >> 8);
                bytes[0] = (byte)result.Value[i * 4 + 3];
                values[i] = BitConverter.ToDouble(bytes, 0);
            }
            return OperationResult<double[]>.Success(values);
        }
        return OperationResult<double[]>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    public override async Task<OperationResult<string>> ReadStringAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var result = await ReadByteArrayAsync(address, length, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            var str = Encoding.ASCII.GetString(result.Value).TrimEnd('\0');
            return OperationResult<string>.Success(str);
        }
        return OperationResult<string>.Failure(result.ErrorMessage ?? "Read failed", result.ErrorCode);
    }

    #endregion

    #region Write Operations

    public override Task<OperationResult> WriteBoolAsync(string address, bool value, CancellationToken cancellationToken = default)
    {
        var (_, addr) = ParseModbusAddress(address, true);
        return WriteSingleCoilAsync(addr, value, cancellationToken);
    }

    public override async Task<OperationResult> WriteBoolArrayAsync(string address, bool[] values, CancellationToken cancellationToken = default)
    {
        // Write coils one at a time for simplicity
        var (_, addr) = ParseModbusAddress(address, true);
        for (int i = 0; i < values.Length; i++)
        {
            var result = await WriteSingleCoilAsync((ushort)(addr + i), values[i], cancellationToken);
            if (!result.IsSuccess)
            {
                return result;
            }
        }
        return OperationResult.Success();
    }

    public override async Task<OperationResult> WriteByteAsync(string address, byte value, CancellationToken cancellationToken = default)
    {
        return await WriteUInt16Async(address, value, cancellationToken);
    }

    public override async Task<OperationResult> WriteByteArrayAsync(string address, byte[] values, CancellationToken cancellationToken = default)
    {
        var registers = new ushort[(values.Length + 1) / 2];
        for (int i = 0; i < registers.Length; i++)
        {
            registers[i] = (ushort)(values[i * 2] << 8);
            if (i * 2 + 1 < values.Length)
            {
                registers[i] |= values[i * 2 + 1];
            }
        }
        return await WriteUInt16ArrayAsync(address, registers, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt16Async(string address, short value, CancellationToken cancellationToken = default)
    {
        return await WriteUInt16Async(address, (ushort)value, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt16ArrayAsync(string address, short[] values, CancellationToken cancellationToken = default)
    {
        return await WriteUInt16ArrayAsync(address, values.Select(v => (ushort)v).ToArray(), cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt16Async(string address, ushort value, CancellationToken cancellationToken = default)
    {
        var (_, addr) = ParseModbusAddress(address, true);
        return await WriteSingleRegisterAsync(addr, value, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt16ArrayAsync(string address, ushort[] values, CancellationToken cancellationToken = default)
    {
        var (_, addr) = ParseModbusAddress(address, true);
        return await WriteRegistersAsync(addr, values, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt32Async(string address, int value, CancellationToken cancellationToken = default)
    {
        var registers = new ushort[2];
        registers[0] = (ushort)(value >> 16);
        registers[1] = (ushort)value;
        return await WriteUInt16ArrayAsync(address, registers, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt32ArrayAsync(string address, int[] values, CancellationToken cancellationToken = default)
    {
        var registers = new ushort[values.Length * 2];
        for (int i = 0; i < values.Length; i++)
        {
            registers[i * 2] = (ushort)(values[i] >> 16);
            registers[i * 2 + 1] = (ushort)values[i];
        }
        return await WriteUInt16ArrayAsync(address, registers, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt32Async(string address, uint value, CancellationToken cancellationToken = default)
    {
        return await WriteInt32Async(address, (int)value, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt32ArrayAsync(string address, uint[] values, CancellationToken cancellationToken = default)
    {
        return await WriteInt32ArrayAsync(address, values.Select(v => (int)v).ToArray(), cancellationToken);
    }

    public override async Task<OperationResult> WriteInt64Async(string address, long value, CancellationToken cancellationToken = default)
    {
        var registers = new ushort[4];
        registers[0] = (ushort)(value >> 48);
        registers[1] = (ushort)(value >> 32);
        registers[2] = (ushort)(value >> 16);
        registers[3] = (ushort)value;
        return await WriteUInt16ArrayAsync(address, registers, cancellationToken);
    }

    public override async Task<OperationResult> WriteInt64ArrayAsync(string address, long[] values, CancellationToken cancellationToken = default)
    {
        var registers = new ushort[values.Length * 4];
        for (int i = 0; i < values.Length; i++)
        {
            registers[i * 4] = (ushort)(values[i] >> 48);
            registers[i * 4 + 1] = (ushort)(values[i] >> 32);
            registers[i * 4 + 2] = (ushort)(values[i] >> 16);
            registers[i * 4 + 3] = (ushort)values[i];
        }
        return await WriteUInt16ArrayAsync(address, registers, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt64Async(string address, ulong value, CancellationToken cancellationToken = default)
    {
        return await WriteInt64Async(address, (long)value, cancellationToken);
    }

    public override async Task<OperationResult> WriteUInt64ArrayAsync(string address, ulong[] values, CancellationToken cancellationToken = default)
    {
        return await WriteInt64ArrayAsync(address, values.Select(v => (long)v).ToArray(), cancellationToken);
    }

    public override async Task<OperationResult> WriteFloatAsync(string address, float value, CancellationToken cancellationToken = default)
    {
        var bytes = BitConverter.GetBytes(value);
        var registers = new ushort[2];
        registers[0] = (ushort)((bytes[3] << 8) | bytes[2]);
        registers[1] = (ushort)((bytes[1] << 8) | bytes[0]);
        return await WriteUInt16ArrayAsync(address, registers, cancellationToken);
    }

    public override async Task<OperationResult> WriteFloatArrayAsync(string address, float[] values, CancellationToken cancellationToken = default)
    {
        var registers = new ushort[values.Length * 2];
        for (int i = 0; i < values.Length; i++)
        {
            var bytes = BitConverter.GetBytes(values[i]);
            registers[i * 2] = (ushort)((bytes[3] << 8) | bytes[2]);
            registers[i * 2 + 1] = (ushort)((bytes[1] << 8) | bytes[0]);
        }
        return await WriteUInt16ArrayAsync(address, registers, cancellationToken);
    }

    public override async Task<OperationResult> WriteDoubleAsync(string address, double value, CancellationToken cancellationToken = default)
    {
        var bytes = BitConverter.GetBytes(value);
        var registers = new ushort[4];
        registers[0] = (ushort)((bytes[7] << 8) | bytes[6]);
        registers[1] = (ushort)((bytes[5] << 8) | bytes[4]);
        registers[2] = (ushort)((bytes[3] << 8) | bytes[2]);
        registers[3] = (ushort)((bytes[1] << 8) | bytes[0]);
        return await WriteUInt16ArrayAsync(address, registers, cancellationToken);
    }

    public override async Task<OperationResult> WriteDoubleArrayAsync(string address, double[] values, CancellationToken cancellationToken = default)
    {
        var registers = new ushort[values.Length * 4];
        for (int i = 0; i < values.Length; i++)
        {
            var bytes = BitConverter.GetBytes(values[i]);
            registers[i * 4] = (ushort)((bytes[7] << 8) | bytes[6]);
            registers[i * 4 + 1] = (ushort)((bytes[5] << 8) | bytes[4]);
            registers[i * 4 + 2] = (ushort)((bytes[3] << 8) | bytes[2]);
            registers[i * 4 + 3] = (ushort)((bytes[1] << 8) | bytes[0]);
        }
        return await WriteUInt16ArrayAsync(address, registers, cancellationToken);
    }

    public override async Task<OperationResult> WriteStringAsync(string address, string value, CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        return await WriteByteArrayAsync(address, bytes, cancellationToken);
    }

    #endregion
}
