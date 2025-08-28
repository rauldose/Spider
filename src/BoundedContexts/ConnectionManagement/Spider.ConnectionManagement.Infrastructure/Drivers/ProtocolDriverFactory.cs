using Microsoft.Extensions.Logging;
using Spider.ConnectionManagement.Application.Interfaces;
using Spider.ConnectionManagement.Domain.ValueObjects;

namespace Spider.ConnectionManagement.Infrastructure.Drivers;

public class ProtocolDriverFactory : IProtocolDriverFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProtocolDriverFactory> _logger;
    private readonly Dictionary<string, Type> _driverTypes;

    public ProtocolDriverFactory(IServiceProvider serviceProvider, ILogger<ProtocolDriverFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _driverTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "Modbus", typeof(ModbusDriver) },
            { "OpcUa", typeof(OpcUaDriver) },
            { "Mqtt", typeof(MqttDriver) },
            { "EthernetIp", typeof(EthernetIpDriver) },
            { "Siemens", typeof(SiemensDriver) },
            { "Omron", typeof(OmronDriver) },
            { "Mitsubishi", typeof(MitsubishiDriver) }
        };
    }

    public IProtocolDriver CreateDriver(string protocolType)
    {
        if (!_driverTypes.TryGetValue(protocolType, out var driverType))
        {
            throw new ArgumentException($"Unsupported protocol type: {protocolType}", nameof(protocolType));
        }

        var loggerFactory = _serviceProvider.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)) as Microsoft.Extensions.Logging.ILoggerFactory;
        var logger = loggerFactory?.CreateLogger(driverType);
        var driver = (IProtocolDriver)Activator.CreateInstance(driverType, logger)!;
        _logger.LogDebug("Created protocol driver for {ProtocolType}", protocolType);
        
        return driver;
    }

    public IEnumerable<string> GetSupportedProtocols()
    {
        return _driverTypes.Keys;
    }
}

// Base class for protocol drivers
public abstract class BaseProtocolDriver : IProtocolDriver
{
    protected readonly ILogger _logger;

    protected BaseProtocolDriver(ILogger logger)
    {
        _logger = logger;
    }

    public abstract string ProtocolType { get; }

    public virtual async Task<bool> TestConnectionAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Testing {ProtocolType} connection to {Host}:{Port}", ProtocolType, parameters.Host, parameters.Port);
            
            using var connection = await ConnectAsync(parameters, cancellationToken);
            var result = connection.IsConnected;
            
            _logger.LogDebug("{ProtocolType} connection test result: {Result}", ProtocolType, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to test {ProtocolType} connection to {Host}:{Port}", ProtocolType, parameters.Host, parameters.Port);
            return false;
        }
    }

    public abstract Task<IConnection> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default);
}

// Stub implementations for basic drivers - these would be replaced with actual protocol implementations
public class ModbusDriver : BaseProtocolDriver
{
    public ModbusDriver(ILogger<ModbusDriver> logger) : base(logger) { }
    public override string ProtocolType => "Modbus";

    public override async Task<IConnection> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default)
    {
        // Simulate connection attempt
        await Task.Delay(100, cancellationToken);
        return new StubConnection("Modbus", parameters, _logger);
    }
}

public class OpcUaDriver : BaseProtocolDriver
{
    public OpcUaDriver(ILogger<OpcUaDriver> logger) : base(logger) { }
    public override string ProtocolType => "OpcUa";

    public override async Task<IConnection> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.Delay(200, cancellationToken);
        return new StubConnection("OpcUa", parameters, _logger);
    }
}

public class MqttDriver : BaseProtocolDriver
{
    public MqttDriver(ILogger<MqttDriver> logger) : base(logger) { }
    public override string ProtocolType => "Mqtt";

    public override async Task<IConnection> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.Delay(150, cancellationToken);
        return new StubConnection("Mqtt", parameters, _logger);
    }
}

public class EthernetIpDriver : BaseProtocolDriver
{
    public EthernetIpDriver(ILogger<EthernetIpDriver> logger) : base(logger) { }
    public override string ProtocolType => "EthernetIp";

    public override async Task<IConnection> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.Delay(120, cancellationToken);
        return new StubConnection("EthernetIp", parameters, _logger);
    }
}

public class SiemensDriver : BaseProtocolDriver
{
    public SiemensDriver(ILogger<SiemensDriver> logger) : base(logger) { }
    public override string ProtocolType => "Siemens";

    public override async Task<IConnection> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.Delay(180, cancellationToken);
        return new StubConnection("Siemens", parameters, _logger);
    }
}

public class OmronDriver : BaseProtocolDriver
{
    public OmronDriver(ILogger<OmronDriver> logger) : base(logger) { }
    public override string ProtocolType => "Omron";

    public override async Task<IConnection> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.Delay(160, cancellationToken);
        return new StubConnection("Omron", parameters, _logger);
    }
}

public class MitsubishiDriver : BaseProtocolDriver
{
    public MitsubishiDriver(ILogger<MitsubishiDriver> logger) : base(logger) { }
    public override string ProtocolType => "Mitsubishi";

    public override async Task<IConnection> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.Delay(140, cancellationToken);
        return new StubConnection("Mitsubishi", parameters, _logger);
    }
}

// Stub connection implementation for testing
public class StubConnection : IConnection
{
    private readonly string _protocolType;
    private readonly ConnectionParameters _parameters;
    private readonly ILogger _logger;
    private bool _isConnected;
    private bool _disposed;

    public StubConnection(string protocolType, ConnectionParameters parameters, ILogger logger)
    {
        _protocolType = protocolType;
        _parameters = parameters;
        _logger = logger;
        _isConnected = true; // Simulate successful connection
        Id = Guid.NewGuid().ToString();
        Health = ConnectionHealth.Healthy(Random.Shared.NextDouble() * 100);
    }

    public string Id { get; }
    public bool IsConnected => _isConnected && !_disposed;
    public ConnectionHealth Health { get; private set; }

    public event EventHandler<ConnectionHealthChangedEventArgs>? HealthChanged;

    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || !_isConnected) return false;

        try
        {
            // Simulate ping
            await Task.Delay(50, cancellationToken);
            var responseTime = Random.Shared.NextDouble() * 200;
            Health = ConnectionHealth.Healthy(responseTime);
            
            HealthChanged?.Invoke(this, new ConnectionHealthChangedEventArgs(Health));
            return true;
        }
        catch (Exception ex)
        {
            var errorHealth = ConnectionHealth.Unhealthy(ex.Message);
            Health = errorHealth;
            HealthChanged?.Invoke(this, new ConnectionHealthChangedEventArgs(Health, ex.Message));
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_disposed) return;

        _logger.LogDebug("Disconnecting {ProtocolType} connection {Id}", _protocolType, Id);
        _isConnected = false;
        await Task.Delay(50); // Simulate cleanup
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _isConnected = false;
        _logger.LogDebug("Disposed {ProtocolType} connection {Id}", _protocolType, Id);
    }
}