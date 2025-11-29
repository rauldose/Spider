using Spider.Drivers.Communication.Models;

namespace Spider.Drivers.Communication.Interfaces;

/// <summary>
/// Common interface for all PLC communication drivers
/// </summary>
public interface IPlcDriver : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the name of the driver type
    /// </summary>
    string DriverTypeName { get; }

    /// <summary>
    /// Gets whether the driver is currently connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event raised when connection status changes
    /// </summary>
    event EventHandler<bool>? ConnectionStatusChanged;

    /// <summary>
    /// Connect to the PLC
    /// </summary>
    /// <returns>Operation result indicating success or failure</returns>
    Task<OperationResult> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from the PLC
    /// </summary>
    /// <returns>Operation result indicating success or failure</returns>
    Task<OperationResult> DisconnectAsync(CancellationToken cancellationToken = default);

    #region Read Operations

    /// <summary>
    /// Read a boolean value from the specified address
    /// </summary>
    Task<OperationResult<bool>> ReadBoolAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read multiple boolean values from the specified address
    /// </summary>
    Task<OperationResult<bool[]>> ReadBoolArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a byte value from the specified address
    /// </summary>
    Task<OperationResult<byte>> ReadByteAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read multiple bytes from the specified address
    /// </summary>
    Task<OperationResult<byte[]>> ReadByteArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a 16-bit signed integer from the specified address
    /// </summary>
    Task<OperationResult<short>> ReadInt16Async(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read multiple 16-bit signed integers from the specified address
    /// </summary>
    Task<OperationResult<short[]>> ReadInt16ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a 16-bit unsigned integer from the specified address
    /// </summary>
    Task<OperationResult<ushort>> ReadUInt16Async(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read multiple 16-bit unsigned integers from the specified address
    /// </summary>
    Task<OperationResult<ushort[]>> ReadUInt16ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a 32-bit signed integer from the specified address
    /// </summary>
    Task<OperationResult<int>> ReadInt32Async(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read multiple 32-bit signed integers from the specified address
    /// </summary>
    Task<OperationResult<int[]>> ReadInt32ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a 32-bit unsigned integer from the specified address
    /// </summary>
    Task<OperationResult<uint>> ReadUInt32Async(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read multiple 32-bit unsigned integers from the specified address
    /// </summary>
    Task<OperationResult<uint[]>> ReadUInt32ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a 64-bit signed integer from the specified address
    /// </summary>
    Task<OperationResult<long>> ReadInt64Async(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read multiple 64-bit signed integers from the specified address
    /// </summary>
    Task<OperationResult<long[]>> ReadInt64ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a 64-bit unsigned integer from the specified address
    /// </summary>
    Task<OperationResult<ulong>> ReadUInt64Async(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read multiple 64-bit unsigned integers from the specified address
    /// </summary>
    Task<OperationResult<ulong[]>> ReadUInt64ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a single-precision floating point value from the specified address
    /// </summary>
    Task<OperationResult<float>> ReadFloatAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read multiple single-precision floating point values from the specified address
    /// </summary>
    Task<OperationResult<float[]>> ReadFloatArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a double-precision floating point value from the specified address
    /// </summary>
    Task<OperationResult<double>> ReadDoubleAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read multiple double-precision floating point values from the specified address
    /// </summary>
    Task<OperationResult<double[]>> ReadDoubleArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read a string from the specified address
    /// </summary>
    Task<OperationResult<string>> ReadStringAsync(string address, ushort length, CancellationToken cancellationToken = default);

    #endregion

    #region Write Operations

    /// <summary>
    /// Write a boolean value to the specified address
    /// </summary>
    Task<OperationResult> WriteBoolAsync(string address, bool value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write multiple boolean values to the specified address
    /// </summary>
    Task<OperationResult> WriteBoolArrayAsync(string address, bool[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a byte value to the specified address
    /// </summary>
    Task<OperationResult> WriteByteAsync(string address, byte value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write multiple bytes to the specified address
    /// </summary>
    Task<OperationResult> WriteByteArrayAsync(string address, byte[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a 16-bit signed integer to the specified address
    /// </summary>
    Task<OperationResult> WriteInt16Async(string address, short value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write multiple 16-bit signed integers to the specified address
    /// </summary>
    Task<OperationResult> WriteInt16ArrayAsync(string address, short[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a 16-bit unsigned integer to the specified address
    /// </summary>
    Task<OperationResult> WriteUInt16Async(string address, ushort value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write multiple 16-bit unsigned integers to the specified address
    /// </summary>
    Task<OperationResult> WriteUInt16ArrayAsync(string address, ushort[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a 32-bit signed integer to the specified address
    /// </summary>
    Task<OperationResult> WriteInt32Async(string address, int value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write multiple 32-bit signed integers to the specified address
    /// </summary>
    Task<OperationResult> WriteInt32ArrayAsync(string address, int[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a 32-bit unsigned integer to the specified address
    /// </summary>
    Task<OperationResult> WriteUInt32Async(string address, uint value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write multiple 32-bit unsigned integers to the specified address
    /// </summary>
    Task<OperationResult> WriteUInt32ArrayAsync(string address, uint[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a 64-bit signed integer to the specified address
    /// </summary>
    Task<OperationResult> WriteInt64Async(string address, long value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write multiple 64-bit signed integers to the specified address
    /// </summary>
    Task<OperationResult> WriteInt64ArrayAsync(string address, long[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a 64-bit unsigned integer to the specified address
    /// </summary>
    Task<OperationResult> WriteUInt64Async(string address, ulong value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write multiple 64-bit unsigned integers to the specified address
    /// </summary>
    Task<OperationResult> WriteUInt64ArrayAsync(string address, ulong[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a single-precision floating point value to the specified address
    /// </summary>
    Task<OperationResult> WriteFloatAsync(string address, float value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write multiple single-precision floating point values to the specified address
    /// </summary>
    Task<OperationResult> WriteFloatArrayAsync(string address, float[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a double-precision floating point value to the specified address
    /// </summary>
    Task<OperationResult> WriteDoubleAsync(string address, double value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write multiple double-precision floating point values to the specified address
    /// </summary>
    Task<OperationResult> WriteDoubleArrayAsync(string address, double[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a string to the specified address
    /// </summary>
    Task<OperationResult> WriteStringAsync(string address, string value, CancellationToken cancellationToken = default);

    #endregion
}
