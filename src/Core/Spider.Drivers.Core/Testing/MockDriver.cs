using Microsoft.Extensions.Logging;
using Spider.Drivers.Core.Abstractions;
using Spider.Drivers.Core.Base;
using Spider.Drivers.Core.Models;

namespace Spider.Drivers.Core.Testing;

/// <summary>
/// Mock driver implementation for testing and demonstration purposes
/// Integrates with the Communication bounded context
/// </summary>
public class MockDriver : BaseDriver, IReadableDriver, IWritableDriver, ISubscribableDriver
{
    private readonly Dictionary<string, object> _dataPoints = new();
    private readonly Timer _dataSimulationTimer;
    private readonly Random _random = new();
    private readonly object _lock = new();

    public MockDriver(ILogger<MockDriver> logger) : base(logger)
    {
        // Initialize some default data points
        InitializeDefaultDataPoints();

        // Start data simulation
        _dataSimulationTimer = new Timer(SimulateDataChanges, null, 
            TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
    }

    public override DriverMetadata Metadata => new DriverMetadata(
        "Mock Driver",
        "1.0.0",
        "Mock driver for testing and demonstration",
        "Spider IoT Platform",
        new[] { "Mock" },
        new DateTime(2024, 1, 1),
        "Spider Development Team");

    public override DriverCapabilities Capabilities => new DriverCapabilities(
        supportsReading: true,
        supportsWriting: true,
        supportsSubscriptions: true,
        supportsRealTime: true,
        supportsDiagnostics: true,
        supportsBulkOperations: true,
        maxConcurrentConnections: 1,
        maxSubscriptions: 100,
        minPollingInterval: TimeSpan.FromMilliseconds(100),
        maxPollingInterval: TimeSpan.FromMinutes(5),
        supportedDataTypes: new[] { "Boolean", "Int16", "Int32", "Float", "String" });

    public bool SupportsAsyncReading => true;
    public bool SupportsAsyncWriting => true;
    public int MaxSubscriptions => 100;

    public event EventHandler<DataChangedEventArgs>? DataChanged;

    protected override async Task<DriverInitializationResult> InitializeDriverAsync(
        DriverConfiguration configuration, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Simulate initialization delay
            await Task.Delay(500, cancellationToken);

            // Parse configuration
            var deviceId = configuration.Parameters.TryGetValue("DeviceId", out var id) 
                ? id.ToString() : "MockDevice01";
            
            var pollingInterval = configuration.Parameters.TryGetValue("PollingInterval", out var interval) 
                ? Convert.ToInt32(interval) : 1000;

            var dataPointCount = configuration.Parameters.TryGetValue("DataPointCount", out var count) 
                ? Convert.ToInt32(count) : 10;

            var enableRandomValues = configuration.Parameters.TryGetValue("EnableRandomValues", out var enable) 
                ? Convert.ToBoolean(enable) : true;

            // Initialize data points based on configuration
            InitializeConfiguredDataPoints(deviceId, dataPointCount, enableRandomValues);

            _logger.LogInformation("Mock driver initialized successfully for device {DeviceId}", deviceId);
            
            var initInfo = new Dictionary<string, object>
            {
                { "DeviceId", deviceId },
                { "DataPointCount", dataPointCount },
                { "PollingInterval", pollingInterval }
            };

            return DriverInitializationResult.CreateSuccess(initInfo, TimeSpan.FromMilliseconds(500));
        }
        catch (Exception ex)
        {
            return DriverInitializationResult.CreateFailure($"Mock driver initialization failed: {ex.Message}");
        }
    }

    protected override async Task<DriverHealthCheckResult> PerformHealthCheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Simulate health check
            await Task.Delay(50, cancellationToken);
            
            var metrics = new Dictionary<string, object>
            {
                { "DataPointCount", _dataPoints.Count },
                { "SimulationActive", true },
                { "LastSimulation", DateTime.UtcNow }
            };

            return DriverHealthCheckResult.CreateHealthy("Mock driver is operational", metrics);
        }
        catch (Exception ex)
        {
            return DriverHealthCheckResult.CreateUnhealthy("Error", ex.Message);
        }
    }

    protected override async Task ShutdownDriverAsync(CancellationToken cancellationToken)
    {
        _dataSimulationTimer?.Dispose();
        
        lock (_lock)
        {
            _dataPoints.Clear();
        }

        _logger.LogInformation("Mock driver shutdown completed");
        await Task.CompletedTask;
    }

    protected override void DisposeDriver()
    {
        _dataSimulationTimer?.Dispose();
    }

    public async Task<ReadOperationResult> ReadAsync(ReadRequest request, CancellationToken cancellationToken = default)
    {
        if (Status != DriverStatus.Ready)
        {
            return ReadOperationResult.CreateFailure("Driver not ready");
        }

        try
        {
            object? value;
            lock (_lock)
            {
                if (_dataPoints.TryGetValue(request.Address, out value))
                {
                    // Found existing value
                }
                else
                {
                    // Create a new data point if it doesn't exist
                    value = GenerateRandomValue(request.DataType);
                    _dataPoints[request.Address] = value;
                }
            }
            
            // Simulate network delay (outside the lock)
            await Task.Delay(_random.Next(10, 50), cancellationToken);

            TrackOperation(true, TimeSpan.FromMilliseconds(_random.Next(5, 25)));
            return ReadOperationResult.CreateSuccess(value, "Good");
        }
        catch (Exception ex)
        {
            TrackOperation(false, TimeSpan.Zero);
            return ReadOperationResult.CreateFailure($"Read operation failed: {ex.Message}");
        }
    }

    public async Task<BulkReadOperationResult> BulkReadAsync(IEnumerable<ReadRequest> requests, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, ReadOperationResult>();
        
        foreach (var request in requests)
        {
            var result = await ReadAsync(request, cancellationToken);
            results[request.Address] = result;
        }

        return BulkReadOperationResult.CreateSuccess(results);
    }

    public async Task<WriteOperationResult> WriteAsync(WriteRequest request, CancellationToken cancellationToken = default)
    {
        if (Status != DriverStatus.Ready)
        {
            return WriteOperationResult.CreateFailure("Driver not ready");
        }

        try
        {
            lock (_lock)
            {
                _dataPoints[request.Address] = request.Value;
            }

            // Simulate write delay (outside the lock)
            await Task.Delay(_random.Next(5, 20), cancellationToken);

            TrackOperation(true, TimeSpan.FromMilliseconds(_random.Next(2, 15)));
            return WriteOperationResult.CreateSuccess();
        }
        catch (Exception ex)
        {
            TrackOperation(false, TimeSpan.Zero);
            return WriteOperationResult.CreateFailure($"Write operation failed: {ex.Message}");
        }
    }

    public async Task<BulkWriteOperationResult> BulkWriteAsync(IEnumerable<WriteRequest> requests, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, WriteOperationResult>();
        
        foreach (var request in requests)
        {
            var result = await WriteAsync(request, cancellationToken);
            results[request.Address] = result;
        }

        return BulkWriteOperationResult.CreateSuccess(results);
    }

    public Task<SubscriptionResult> SubscribeAsync(SubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionId = Guid.NewGuid().ToString();
            // In a real implementation, you would track subscriptions
            return Task.FromResult(SubscriptionResult.CreateSuccess(subscriptionId));
        }
        catch (Exception ex)
        {
            return Task.FromResult(SubscriptionResult.CreateFailure("", $"Subscription failed: {ex.Message}"));
        }
    }

    public Task<UnsubscriptionResult> UnsubscribeAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, you would remove the subscription
            return Task.FromResult(UnsubscriptionResult.CreateSuccess(subscriptionId));
        }
        catch (Exception ex)
        {
            return Task.FromResult(UnsubscriptionResult.CreateFailure("", $"Unsubscription failed: {ex.Message}"));
        }
    }

    private void InitializeDefaultDataPoints()
    {
        lock (_lock)
        {
            // Initialize some default data points
            for (int i = 0; i < 10; i++)
            {
                var address = $"DB1.DBD{i * 4}";
                _dataPoints[address] = _random.Next(0, 1000);
            }

            // Add some boolean values
            for (int i = 0; i < 5; i++)
            {
                var address = $"M{i}.0";
                _dataPoints[address] = _random.Next(2) == 1;
            }

            // Add some float values
            for (int i = 0; i < 5; i++)
            {
                var address = $"DB2.DBD{i * 4}";
                _dataPoints[address] = (float)(_random.NextDouble() * 100);
            }
        }
    }

    private void InitializeConfiguredDataPoints(string deviceId, int count, bool enableRandomValues)
    {
        lock (_lock)
        {
            _dataPoints.Clear();

            for (int i = 0; i < count; i++)
            {
                var address = $"{deviceId}.Point{i:D3}";
                
                if (enableRandomValues)
                {
                    // Mix of different data types
                    if (i % 3 == 0)
                        _dataPoints[address] = _random.Next(0, 1000); // Integer
                    else if (i % 3 == 1)
                        _dataPoints[address] = _random.Next(2) == 1; // Boolean
                    else
                        _dataPoints[address] = (float)(_random.NextDouble() * 100); // Float
                }
                else
                {
                    _dataPoints[address] = i; // Sequential values
                }
            }
        }
    }

    private object GenerateRandomValue(string dataType)
    {
        return dataType?.ToLowerInvariant() switch
        {
            "boolean" => _random.Next(2) == 1,
            "int16" => (short)_random.Next(-32768, 32767),
            "int32" => _random.Next(-1000000, 1000000),
            "float" => (float)(_random.NextDouble() * 1000),
            "double" => _random.NextDouble() * 1000,
            "string" => $"MockValue_{_random.Next(1000, 9999)}",
            _ => _random.Next(0, 1000)
        };
    }

    private void SimulateDataChanges(object? state)
    {
        if (Status != DriverStatus.Ready)
            return;

        try
        {
            lock (_lock)
            {
                if (!_dataPoints.Any())
                    return;

                // Update a few random data points
                var pointsToUpdate = Math.Min(3, _dataPoints.Count);
                var addresses = _dataPoints.Keys.OrderBy(x => _random.Next()).Take(pointsToUpdate);

                foreach (var address in addresses)
                {
                    var currentValue = _dataPoints[address];
                    var newValue = currentValue switch
                    {
                        int intVal => intVal + _random.Next(-10, 11),
                        float floatVal => floatVal + (float)(_random.NextDouble() - 0.5) * 10,
                        bool boolVal => _random.Next(10) == 0 ? !boolVal : boolVal, // Change occasionally
                        _ => currentValue
                    };

                    _dataPoints[address] = newValue;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash simulation
            _logger.LogWarning(ex, "Data simulation error");
        }
    }
}