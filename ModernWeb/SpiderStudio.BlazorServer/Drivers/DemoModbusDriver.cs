using SpiderDriver.UnifiedAPI.Interfaces;
using SpiderDriver.UnifiedAPI.Models;

namespace SpiderStudio.BlazorServer.Drivers;

/// <summary>
/// Demo implementation of the unified driver interface
/// This shows how existing Spider drivers can be adapted to the new interface
/// </summary>
public class DemoModbusDriver : IUnifiedDriver
{
    private ConnectionStatus _status = ConnectionStatus.Disconnected;
    private readonly Dictionary<string, object> _dataCache = new();
    private readonly Timer _simulationTimer;

    public DemoModbusDriver()
    {
        Id = Guid.NewGuid().ToString();
        Name = "Demo Modbus Driver";
        
        // Simulate data changes
        _simulationTimer = new Timer(SimulateDataChanges, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
    }

    public string Id { get; }
    public string Name { get; set; }
    public string Description => "Demo Modbus TCP/RTU driver for testing unified interface";
    public string ProtocolType => "Modbus";
    public IEnumerable<string> SupportedChannels => new[] { "TCP", "RTU", "ASCII" };
    public IEnumerable<string> SupportedRegisters => new[] { "Coils", "DiscreteInputs", "HoldingRegisters", "InputRegisters" };
    public ConnectionStatus Status => _status;
    public DriverCapabilities Capabilities => DriverCapabilities.Read | DriverCapabilities.Write | DriverCapabilities.Subscribe | DriverCapabilities.BulkRead;

    public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
    public event EventHandler<DataValueChangedEventArgs>? DataValueChanged;
    public event EventHandler<DriverErrorEventArgs>? DriverError;

    public Task<bool> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default)
    {
        SetStatus(ConnectionStatus.Connecting);
        
        // Simulate connection delay
        return Task.Run(async () =>
        {
            await Task.Delay(1000, cancellationToken);
            
            // Simulate successful connection
            SetStatus(ConnectionStatus.Connected);
            return true;
        }, cancellationToken);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        SetStatus(ConnectionStatus.Disconnected);
        _dataCache.Clear();
        return Task.CompletedTask;
    }

    public Task<ReadResult> ReadAsync(ReadRequest request, CancellationToken cancellationToken = default)
    {
        if (_status != ConnectionStatus.Connected)
        {
            return Task.FromResult(new ReadResult
            {
                IsSuccess = false,
                ErrorMessage = "Driver not connected"
            });
        }

        var values = new Dictionary<string, object>();
        
        foreach (var address in request.Addresses)
        {
            // Simulate reading values
            values[address] = GenerateSimulatedValue(address, request.RegisterType);
        }

        return Task.FromResult(new ReadResult
        {
            IsSuccess = true,
            Values = values
        });
    }

    public Task<WriteResult> WriteAsync(WriteRequest request, CancellationToken cancellationToken = default)
    {
        if (_status != ConnectionStatus.Connected)
        {
            return Task.FromResult(new WriteResult
            {
                IsSuccess = false,
                ErrorMessage = "Driver not connected"
            });
        }

        // Simulate writing values
        foreach (var kvp in request.Values)
        {
            _dataCache[kvp.Key] = kvp.Value;
        }

        return Task.FromResult(new WriteResult { IsSuccess = true });
    }

    public Task<IDataSubscription> SubscribeAsync(SubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var subscription = new DemoDataSubscription(request.Addresses, this);
        return Task.FromResult<IDataSubscription>(subscription);
    }

    public DriverConfigurationSchema GetConfigurationSchema()
    {
        return new DriverConfigurationSchema
        {
            DriverType = "Modbus",
            Properties = new List<ConfigurationProperty>
            {
                new() { Name = "Host", DisplayName = "Host/IP Address", PropertyType = typeof(string), IsRequired = true, DefaultValue = "localhost" },
                new() { Name = "Port", DisplayName = "Port", PropertyType = typeof(int), IsRequired = true, DefaultValue = 502 },
                new() { Name = "SlaveId", DisplayName = "Slave ID", PropertyType = typeof(byte), IsRequired = true, DefaultValue = (byte)1 },
                new() { Name = "Timeout", DisplayName = "Timeout (ms)", PropertyType = typeof(int), IsRequired = false, DefaultValue = 1000 }
            },
            RegisterTypes = new List<RegisterTypeDefinition>
            {
                new() { Name = "Coils", Description = "Read/Write Coils (0x)", AddressFormat = "0-65535", SupportedDataTypes = new List<string> { "bool" } },
                new() { Name = "DiscreteInputs", Description = "Read Discrete Inputs (1x)", AddressFormat = "10001-165536", SupportedDataTypes = new List<string> { "bool" } },
                new() { Name = "InputRegisters", Description = "Read Input Registers (3x)", AddressFormat = "30001-365536", SupportedDataTypes = new List<string> { "short", "int", "float" } },
                new() { Name = "HoldingRegisters", Description = "Read/Write Holding Registers (4x)", AddressFormat = "40001-465536", SupportedDataTypes = new List<string> { "short", "int", "float" } }
            }
        };
    }

    public ValidationResult ValidateTagConfiguration(TagConfiguration tag)
    {
        var result = new ValidationResult { IsValid = true };
        
        if (string.IsNullOrWhiteSpace(tag.Address))
        {
            result.IsValid = false;
            result.Errors.Add("Address is required");
        }
        
        if (!SupportedRegisters.Contains(tag.RegisterType))
        {
            result.IsValid = false;
            result.Errors.Add($"Register type '{tag.RegisterType}' is not supported");
        }

        return result;
    }

    private void SetStatus(ConnectionStatus newStatus)
    {
        var oldStatus = _status;
        _status = newStatus;
        ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(oldStatus, newStatus));
    }

    private object GenerateSimulatedValue(string address, string registerType)
    {
        var random = new Random();
        
        return registerType switch
        {
            "Coils" or "DiscreteInputs" => random.Next(2) == 1,
            "HoldingRegisters" or "InputRegisters" => random.Next(0, 1000),
            _ => random.NextDouble() * 100
        };
    }

    private void SimulateDataChanges(object? state)
    {
        if (_status == ConnectionStatus.Connected && _dataCache.Any())
        {
            var address = _dataCache.Keys.First();
            var oldValue = _dataCache[address];
            var newValue = GenerateSimulatedValue(address, "HoldingRegisters");
            
            _dataCache[address] = newValue;
            DataValueChanged?.Invoke(this, new DataValueChangedEventArgs(address, oldValue, newValue, "HoldingRegisters"));
        }
    }

    public void Dispose()
    {
        _simulationTimer?.Dispose();
    }
}

/// <summary>
/// Demo data subscription implementation
/// </summary>
public class DemoDataSubscription : IDataSubscription
{
    private readonly string[] _addresses;
    private readonly DemoModbusDriver _driver;
    private bool _isDisposed;

    public DemoDataSubscription(string[] addresses, DemoModbusDriver driver)
    {
        Id = Guid.NewGuid().ToString();
        _addresses = addresses;
        _driver = driver;
        IsActive = true;
        
        // Subscribe to driver events
        _driver.DataValueChanged += OnDriverDataValueChanged;
    }

    public string Id { get; }
    public bool IsActive { get; private set; }

    public event EventHandler<DataValueChangedEventArgs>? DataChanged;

    public Task UnsubscribeAsync()
    {
        if (!_isDisposed)
        {
            _driver.DataValueChanged -= OnDriverDataValueChanged;
            IsActive = false;
        }
        return Task.CompletedTask;
    }

    private void OnDriverDataValueChanged(object? sender, DataValueChangedEventArgs e)
    {
        if (_addresses.Contains(e.Address))
        {
            DataChanged?.Invoke(this, e);
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            UnsubscribeAsync().Wait();
            _isDisposed = true;
        }
    }
}