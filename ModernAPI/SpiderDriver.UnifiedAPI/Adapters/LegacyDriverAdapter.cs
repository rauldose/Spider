using Cdy.Spider;
using SpiderDriver.UnifiedAPI.Interfaces;
using SpiderDriver.UnifiedAPI.Models;

namespace SpiderDriver.UnifiedAPI.Adapters;

/// <summary>
/// Adapter to bridge existing Spider IDriverDevelop implementations to the new unified interface
/// This enables gradual migration from the legacy architecture to the modern unified API
/// </summary>
public class LegacyDriverAdapter : IUnifiedDriver
{
    private readonly IDriverDevelop _legacyDriver;
    private ConnectionStatus _status = ConnectionStatus.Disconnected;
    private readonly Dictionary<string, object> _dataCache = new();

    public LegacyDriverAdapter(IDriverDevelop legacyDriver)
    {
        _legacyDriver = legacyDriver ?? throw new ArgumentNullException(nameof(legacyDriver));
        Id = Guid.NewGuid().ToString();
        Name = _legacyDriver.Name ?? "Legacy Driver";
    }

    #region IUnifiedDriver Implementation

    public string Id { get; }
    public string Name { get; set; }
    public string Description => _legacyDriver.Desc ?? "Legacy Spider Driver";
    public string ProtocolType => _legacyDriver.TypeName ?? "Unknown";
    
    public IEnumerable<string> SupportedChannels
    {
        get
        {
            // Map legacy channel support to modern format
            // Legacy drivers don't expose this information directly, so we provide defaults
            return ProtocolType.ToLower() switch
            {
                "modbus" => new[] { "TCP", "RTU", "ASCII" },
                "opcua" => new[] { "TCP" },
                "mqtt" => new[] { "TCP", "WebSocket" },
                _ => new[] { "TCP" }
            };
        }
    }
    
    public IEnumerable<string> SupportedRegisters => _legacyDriver.SupportRegistors ?? Array.Empty<string>();
    public ConnectionStatus Status => _status;
    
    public DriverCapabilities Capabilities
    {
        get
        {
            // Most legacy drivers support read/write operations
            var caps = DriverCapabilities.Read | DriverCapabilities.Write;
            
            // Add protocol-specific capabilities
            if (ProtocolType.Contains("OPC", StringComparison.OrdinalIgnoreCase))
            {
                caps |= DriverCapabilities.Subscribe | DriverCapabilities.Alarms | DriverCapabilities.Events;
            }
            
            if (ProtocolType.Contains("MQTT", StringComparison.OrdinalIgnoreCase))
            {
                caps |= DriverCapabilities.Subscribe;
            }
            
            return caps;
        }
    }

    public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
    public event EventHandler<DataValueChangedEventArgs>? DataValueChanged;
    public event EventHandler<DriverErrorEventArgs>? DriverError;

    public async Task<bool> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            SetStatus(ConnectionStatus.Connecting);
            
            // Legacy drivers don't have async connect methods, so we simulate
            await Task.Run(() =>
            {
                // Legacy connection logic would go here
                // For now, we'll simulate a successful connection
                Thread.Sleep(500); // Simulate connection time
            }, cancellationToken);
            
            SetStatus(ConnectionStatus.Connected);
            return true;
        }
        catch (Exception ex)
        {
            SetStatus(ConnectionStatus.Error);
            DriverError?.Invoke(this, new DriverErrorEventArgs("CONNECTION_FAILED", ex.Message, ex));
            return false;
        }
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        SetStatus(ConnectionStatus.Disconnected);
        _dataCache.Clear();
        return Task.CompletedTask;
    }

    public async Task<ReadResult> ReadAsync(ReadRequest request, CancellationToken cancellationToken = default)
    {
        if (_status != ConnectionStatus.Connected)
        {
            return new ReadResult
            {
                IsSuccess = false,
                ErrorMessage = "Driver not connected"
            };
        }

        try
        {
            var values = new Dictionary<string, object>();
            
            // Legacy drivers typically read one address at a time
            foreach (var address in request.Addresses)
            {
                // Simulate reading from legacy driver
                await Task.Run(() =>
                {
                    // This would call the legacy driver's read method
                    var value = SimulateLegacyRead(address, request.RegisterType);
                    values[address] = value;
                    _dataCache[address] = value;
                }, cancellationToken);
            }
            
            return new ReadResult
            {
                IsSuccess = true,
                Values = values
            };
        }
        catch (Exception ex)
        {
            DriverError?.Invoke(this, new DriverErrorEventArgs("READ_FAILED", ex.Message, ex));
            return new ReadResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<WriteResult> WriteAsync(WriteRequest request, CancellationToken cancellationToken = default)
    {
        if (_status != ConnectionStatus.Connected)
        {
            return new WriteResult
            {
                IsSuccess = false,
                ErrorMessage = "Driver not connected"
            };
        }

        try
        {
            await Task.Run(() =>
            {
                // Legacy write operations
                foreach (var kvp in request.Values)
                {
                    SimulateLegacyWrite(kvp.Key, kvp.Value, request.RegisterType);
                    _dataCache[kvp.Key] = kvp.Value;
                }
            }, cancellationToken);
            
            return new WriteResult { IsSuccess = true };
        }
        catch (Exception ex)
        {
            DriverError?.Invoke(this, new DriverErrorEventArgs("WRITE_FAILED", ex.Message, ex));
            return new WriteResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public Task<IDataSubscription> SubscribeAsync(SubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var subscription = new LegacyDataSubscription(request.Addresses, this, request.UpdateInterval);
        return Task.FromResult<IDataSubscription>(subscription);
    }

    public DriverConfigurationSchema GetConfigurationSchema()
    {
        // Convert legacy driver configuration to modern schema
        var schema = new DriverConfigurationSchema
        {
            DriverType = ProtocolType,
            Properties = GetLegacyConfigurationProperties(),
            RegisterTypes = GetLegacyRegisterTypes()
        };
        
        return schema;
    }

    public ValidationResult ValidateTagConfiguration(TagConfiguration tag)
    {
        var result = new ValidationResult { IsValid = true };
        
        try
        {
            // Use legacy driver's validation if available
            // Legacy drivers typically validate through CheckTagDeviceInfo
            var legacyTag = CreateLegacyTag(tag);
            _legacyDriver.CheckTagDeviceInfo(legacyTag);
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add(ex.Message);
        }
        
        return result;
    }

    #endregion

    #region Private Helper Methods

    private void SetStatus(ConnectionStatus newStatus)
    {
        var oldStatus = _status;
        _status = newStatus;
        ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(oldStatus, newStatus));
    }

    private object SimulateLegacyRead(string address, string registerType)
    {
        // Simulate legacy driver read operation
        var random = new Random();
        
        return registerType.ToLower() switch
        {
            "coils" or "discrete" => random.Next(2) == 1,
            "holding" or "input" => random.Next(0, 1000),
            _ => random.NextDouble() * 100
        };
    }

    private void SimulateLegacyWrite(string address, object value, string registerType)
    {
        // Simulate legacy driver write operation
        // In real implementation, this would call the legacy driver's write method
    }

    private List<ConfigurationProperty> GetLegacyConfigurationProperties()
    {
        // Convert legacy driver configuration to modern properties
        var properties = new List<ConfigurationProperty>();
        
        // Common properties that most legacy drivers have
        properties.Add(new ConfigurationProperty
        {
            Name = "Host",
            DisplayName = "Host/IP Address",
            Description = "The IP address or hostname of the device",
            PropertyType = typeof(string),
            IsRequired = true,
            DefaultValue = "192.168.1.100"
        });
        
        properties.Add(new ConfigurationProperty
        {
            Name = "Port",
            DisplayName = "Port Number",
            Description = "The port number for communication",
            PropertyType = typeof(int),
            IsRequired = true,
            DefaultValue = GetDefaultPort()
        });
        
        return properties;
    }

    private List<RegisterTypeDefinition> GetLegacyRegisterTypes()
    {
        var registerTypes = new List<RegisterTypeDefinition>();
        
        foreach (var register in SupportedRegisters)
        {
            registerTypes.Add(new RegisterTypeDefinition
            {
                Name = register,
                Description = $"Legacy register type: {register}",
                AddressFormat = "Address format depends on protocol",
                SupportedDataTypes = new List<string> { "bool", "short", "int", "float", "string" }
            });
        }
        
        return registerTypes;
    }

    private int GetDefaultPort()
    {
        return ProtocolType.ToLower() switch
        {
            "modbus" => 502,
            "opcua" => 4840,
            "mqtt" => 1883,
            _ => 502
        };
    }

    private Tagbase CreateLegacyTag(TagConfiguration tag)
    {
        // Create a legacy tag object from modern configuration
        // This is a simplified implementation - in practice you'd need to create
        // the appropriate tag type based on the data type
        return new IntTag()
        {
            Name = tag.Name,
            // Address = tag.Address, // Legacy tags may have different address formats
            // Other properties would be mapped here
        };
    }

    #endregion
}

/// <summary>
/// Legacy data subscription implementation
/// </summary>
public class LegacyDataSubscription : IDataSubscription
{
    private readonly string[] _addresses;
    private readonly LegacyDriverAdapter _driver;
    private readonly Timer _updateTimer;
    private bool _isDisposed;

    public LegacyDataSubscription(string[] addresses, LegacyDriverAdapter driver, TimeSpan updateInterval)
    {
        Id = Guid.NewGuid().ToString();
        _addresses = addresses;
        _driver = driver;
        IsActive = true;
        
        // Start periodic updates to simulate legacy driver behavior
        _updateTimer = new Timer(UpdateData, null, updateInterval, updateInterval);
    }

    public string Id { get; }
    public bool IsActive { get; private set; }

    public event EventHandler<DataValueChangedEventArgs>? DataChanged;

    public Task UnsubscribeAsync()
    {
        if (!_isDisposed)
        {
            _updateTimer?.Dispose();
            IsActive = false;
        }
        return Task.CompletedTask;
    }

    private void UpdateData(object? state)
    {
        if (!IsActive || _isDisposed) return;
        
        try
        {
            // Simulate data updates from legacy driver
            var random = new Random();
            foreach (var address in _addresses)
            {
                var newValue = random.NextDouble() * 100;
                DataChanged?.Invoke(this, new DataValueChangedEventArgs(address, null, newValue, "HoldingRegisters"));
            }
        }
        catch
        {
            // Ignore errors in simulation
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            UnsubscribeAsync().Wait();
            _updateTimer?.Dispose();
            _isDisposed = true;
        }
    }
}