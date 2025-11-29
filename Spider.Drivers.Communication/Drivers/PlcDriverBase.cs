using System.Net.Sockets;
using Spider.Drivers.Communication.Interfaces;
using Spider.Drivers.Communication.Models;

namespace Spider.Drivers.Communication.Drivers;

/// <summary>
/// Base class for PLC drivers providing common TCP communication functionality
/// </summary>
public abstract class PlcDriverBase : IPlcDriver
{
    protected TcpClient? _client;
    protected NetworkStream? _stream;
    protected readonly ConnectionSettings _settings;
    protected readonly SemaphoreSlim _communicationLock = new(1, 1);
    protected bool _disposed;

    public abstract string DriverTypeName { get; }
    public bool IsConnected => _client?.Connected ?? false;
    public event EventHandler<bool>? ConnectionStatusChanged;

    protected PlcDriverBase(ConnectionSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public virtual async Task<OperationResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (IsConnected)
            {
                return OperationResult.Success();
            }

            _client = new TcpClient();
            _client.ReceiveTimeout = _settings.ReceiveTimeout;
            _client.SendTimeout = _settings.SendTimeout;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_settings.ConnectTimeout);

            await _client.ConnectAsync(_settings.IpAddress, _settings.Port, cts.Token);
            _stream = _client.GetStream();

            var initResult = await InitializeConnectionAsync(cancellationToken);
            if (!initResult.IsSuccess)
            {
                await DisconnectAsync(cancellationToken);
                return initResult;
            }

            ConnectionStatusChanged?.Invoke(this, true);
            return OperationResult.Success();
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Failure("Connection timeout");
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Connection failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialize the connection after TCP connect (e.g., protocol handshake)
    /// </summary>
    protected virtual Task<OperationResult> InitializeConnectionAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(OperationResult.Success());
    }

    public virtual async Task<OperationResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var wasConnected = IsConnected;

            if (_stream != null)
            {
                await _stream.DisposeAsync();
                _stream = null;
            }

            _client?.Close();
            _client?.Dispose();
            _client = null;

            if (wasConnected)
            {
                ConnectionStatusChanged?.Invoke(this, false);
            }

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Disconnect failed: {ex.Message}");
        }
    }

    protected async Task<byte[]?> SendAndReceiveAsync(byte[] request, int expectedResponseLength, CancellationToken cancellationToken)
    {
        if (!IsConnected || _stream == null)
        {
            return null;
        }

        await _communicationLock.WaitAsync(cancellationToken);
        try
        {
            await _stream.WriteAsync(request, cancellationToken);
            await _stream.FlushAsync(cancellationToken);

            var response = new byte[expectedResponseLength];
            var totalRead = 0;

            while (totalRead < expectedResponseLength)
            {
                var read = await _stream.ReadAsync(response.AsMemory(totalRead, expectedResponseLength - totalRead), cancellationToken);
                if (read == 0)
                {
                    throw new IOException("Connection closed by remote host");
                }
                totalRead += read;
            }

            return response;
        }
        finally
        {
            _communicationLock.Release();
        }
    }

    protected async Task<byte[]?> SendAndReceiveVariableLengthAsync(byte[] request, Func<byte[], int, int> getLengthFromHeader, int headerLength, CancellationToken cancellationToken)
    {
        if (!IsConnected || _stream == null)
        {
            return null;
        }

        await _communicationLock.WaitAsync(cancellationToken);
        try
        {
            await _stream.WriteAsync(request, cancellationToken);
            await _stream.FlushAsync(cancellationToken);

            // Read header first
            var header = new byte[headerLength];
            var totalRead = 0;

            while (totalRead < headerLength)
            {
                var read = await _stream.ReadAsync(header.AsMemory(totalRead, headerLength - totalRead), cancellationToken);
                if (read == 0)
                {
                    throw new IOException("Connection closed by remote host");
                }
                totalRead += read;
            }

            // Get full message length from header
            var fullLength = getLengthFromHeader(header, headerLength);
            var response = new byte[fullLength];
            Array.Copy(header, response, headerLength);

            // Read remaining data
            var remaining = fullLength - headerLength;
            totalRead = 0;
            while (totalRead < remaining)
            {
                var read = await _stream.ReadAsync(response.AsMemory(headerLength + totalRead, remaining - totalRead), cancellationToken);
                if (read == 0)
                {
                    throw new IOException("Connection closed by remote host");
                }
                totalRead += read;
            }

            return response;
        }
        finally
        {
            _communicationLock.Release();
        }
    }

    #region Abstract Read Operations

    public abstract Task<OperationResult<bool>> ReadBoolAsync(string address, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<bool[]>> ReadBoolArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<byte>> ReadByteAsync(string address, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<byte[]>> ReadByteArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<short>> ReadInt16Async(string address, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<short[]>> ReadInt16ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<ushort>> ReadUInt16Async(string address, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<ushort[]>> ReadUInt16ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<int>> ReadInt32Async(string address, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<int[]>> ReadInt32ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<uint>> ReadUInt32Async(string address, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<uint[]>> ReadUInt32ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<long>> ReadInt64Async(string address, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<long[]>> ReadInt64ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<ulong>> ReadUInt64Async(string address, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<ulong[]>> ReadUInt64ArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<float>> ReadFloatAsync(string address, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<float[]>> ReadFloatArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<double>> ReadDoubleAsync(string address, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<double[]>> ReadDoubleArrayAsync(string address, ushort length, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<string>> ReadStringAsync(string address, ushort length, CancellationToken cancellationToken = default);

    #endregion

    #region Abstract Write Operations

    public abstract Task<OperationResult> WriteBoolAsync(string address, bool value, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteBoolArrayAsync(string address, bool[] values, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteByteAsync(string address, byte value, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteByteArrayAsync(string address, byte[] values, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteInt16Async(string address, short value, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteInt16ArrayAsync(string address, short[] values, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteUInt16Async(string address, ushort value, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteUInt16ArrayAsync(string address, ushort[] values, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteInt32Async(string address, int value, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteInt32ArrayAsync(string address, int[] values, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteUInt32Async(string address, uint value, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteUInt32ArrayAsync(string address, uint[] values, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteInt64Async(string address, long value, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteInt64ArrayAsync(string address, long[] values, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteUInt64Async(string address, ulong value, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteUInt64ArrayAsync(string address, ulong[] values, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteFloatAsync(string address, float value, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteFloatArrayAsync(string address, float[] values, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteDoubleAsync(string address, double value, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteDoubleArrayAsync(string address, double[] values, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult> WriteStringAsync(string address, string value, CancellationToken cancellationToken = default);

    #endregion

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _stream?.Dispose();
            _client?.Dispose();
            _communicationLock.Dispose();
        }

        _disposed = true;
    }
}
