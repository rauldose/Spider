using Microsoft.Extensions.Logging;
using Spider.Drivers.Core.Abstractions;
using Spider.Drivers.Core.Models;
using System.Collections.Concurrent;

namespace Spider.Drivers.Core.Implementations;

/// <summary>
/// In-memory implementation of driver repository for demonstration
/// In production, this would be backed by a database
/// </summary>
public class DriverRepository : IDriverRepository
{
    private readonly ConcurrentDictionary<string, IDriver> _drivers = new();
    private readonly ILogger<DriverRepository> _logger;

    public DriverRepository(ILogger<DriverRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IDriver?> GetByIdAsync(string driverId, CancellationToken cancellationToken = default)
    {
        _drivers.TryGetValue(driverId, out var driver);
        return Task.FromResult(driver);
    }

    public Task<IEnumerable<IDriver>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_drivers.Values.AsEnumerable());
    }

    public Task<IEnumerable<IDriver>> GetByProtocolAsync(string protocolType, CancellationToken cancellationToken = default)
    {
        var drivers = _drivers.Values
            .Where(d => d.Metadata.SupportedProtocols.Contains(protocolType, StringComparer.OrdinalIgnoreCase))
            .ToList();
        
        return Task.FromResult(drivers.AsEnumerable());
    }

    public Task<IEnumerable<IDriver>> GetByStatusAsync(DriverStatus status, CancellationToken cancellationToken = default)
    {
        var drivers = _drivers.Values
            .Where(d => d.Status.Id == status.Id)
            .ToList();
        
        return Task.FromResult(drivers.AsEnumerable());
    }

    public Task<string> AddAsync(IDriver driver, CancellationToken cancellationToken = default)
    {
        if (driver == null)
            throw new ArgumentNullException(nameof(driver));

        var driverId = Guid.NewGuid().ToString();
        
        if (_drivers.TryAdd(driverId, driver))
        {
            _logger.LogDebug("Added driver {DriverId} of type {DriverType}", driverId, driver.GetType().Name);
            return Task.FromResult(driverId);
        }
        
        throw new InvalidOperationException($"Failed to add driver {driverId}");
    }

    public Task UpdateAsync(IDriver driver, CancellationToken cancellationToken = default)
    {
        if (driver == null)
            throw new ArgumentNullException(nameof(driver));

        // Find the driver by reference and update it
        var existingEntry = _drivers.FirstOrDefault(kvp => ReferenceEquals(kvp.Value, driver));
        if (existingEntry.Key != null)
        {
            _drivers.TryUpdate(existingEntry.Key, driver, existingEntry.Value);
            _logger.LogDebug("Updated driver {DriverId}", existingEntry.Key);
        }
        else
        {
            throw new InvalidOperationException("Driver not found in repository");
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string driverId, CancellationToken cancellationToken = default)
    {
        if (_drivers.TryRemove(driverId, out var driver))
        {
            _logger.LogDebug("Removed driver {DriverId} of type {DriverType}", driverId, driver.GetType().Name);
            driver.Dispose();
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string driverId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_drivers.ContainsKey(driverId));
    }

    public Task<Dictionary<DriverStatus, int>> GetCountByStatusAsync(CancellationToken cancellationToken = default)
    {
        var statusCounts = _drivers.Values
            .GroupBy(d => d.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult(statusCounts);
    }
}

/// <summary>
/// Driver manager implementation for lifecycle management
/// </summary>
public class DriverManager : IDriverManager, IDisposable
{
    private readonly IDriverFactory _driverFactory;
    private readonly IDriverRepository _driverRepository;
    private readonly ILogger<DriverManager> _logger;
    private readonly ConcurrentDictionary<string, DriverConfiguration> _driverConfigurations = new();
    private readonly ConcurrentDictionary<string, DriverHealthCheckResult> _healthResults = new();
    private readonly Timer? _healthCheckTimer;
    private bool _disposed;

    public DriverManager(
        IDriverFactory driverFactory, 
        IDriverRepository driverRepository, 
        ILogger<DriverManager> logger)
    {
        _driverFactory = driverFactory ?? throw new ArgumentNullException(nameof(driverFactory));
        _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Start health check timer (every 30 seconds)
        _healthCheckTimer = new Timer(PerformPeriodicHealthCheck, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public event EventHandler<DriverAddedEventArgs>? DriverAdded;
    public event EventHandler<DriverRemovedEventArgs>? DriverRemoved;
    public event EventHandler<DriverHealthChangedEventArgs>? DriverHealthChanged;

    public async Task InitializeAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing all drivers");
        
        var drivers = await _driverRepository.GetAllAsync(cancellationToken);
        var initializationTasks = new List<Task>();

        foreach (var driver in drivers)
        {
            var driverId = await GetDriverIdAsync(driver);
            if (driverId != null && _driverConfigurations.TryGetValue(driverId, out var config))
            {
                initializationTasks.Add(InitializeDriverAsync(driver, config, cancellationToken));
            }
        }

        await Task.WhenAll(initializationTasks);
        _logger.LogInformation("Driver initialization completed");
    }

    public async Task ShutdownAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Shutting down all drivers");
        
        var drivers = await _driverRepository.GetAllAsync(cancellationToken);
        var shutdownTasks = drivers.Select(driver => driver.ShutdownAsync(cancellationToken)).ToList();

        await Task.WhenAll(shutdownTasks);
        _logger.LogInformation("Driver shutdown completed");
    }

    public async Task<IDriver?> GetDriverAsync(string driverId, CancellationToken cancellationToken = default)
    {
        return await _driverRepository.GetByIdAsync(driverId, cancellationToken);
    }

    public async Task<string> CreateDriverAsync(string protocolType, DriverConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating driver for protocol {ProtocolType}", protocolType);
            
            var driver = _driverFactory.CreateDriver(protocolType, configuration);
            var driverId = await _driverRepository.AddAsync(driver, cancellationToken);
            
            _driverConfigurations.TryAdd(driverId, configuration);
            
            // Initialize the driver
            var initResult = await driver.InitializeAsync(configuration, cancellationToken);
            if (!initResult.Success)
            {
                _logger.LogWarning("Driver initialization failed: {Error}", initResult.ErrorMessage);
            }

            DriverAdded?.Invoke(this, new DriverAddedEventArgs(driverId, protocolType));
            
            _logger.LogInformation("Driver {DriverId} created successfully for protocol {ProtocolType}", driverId, protocolType);
            return driverId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create driver for protocol {ProtocolType}", protocolType);
            throw;
        }
    }

    public async Task RemoveDriverAsync(string driverId, CancellationToken cancellationToken = default)
    {
        try
        {
            var driver = await _driverRepository.GetByIdAsync(driverId, cancellationToken);
            if (driver == null)
            {
                _logger.LogWarning("Driver {DriverId} not found for removal", driverId);
                return;
            }

            var protocolType = driver.Metadata.SupportedProtocols.FirstOrDefault() ?? "Unknown";
            
            await driver.ShutdownAsync(cancellationToken);
            await _driverRepository.RemoveAsync(driverId, cancellationToken);
            
            _driverConfigurations.TryRemove(driverId, out _);
            _healthResults.TryRemove(driverId, out _);
            
            DriverRemoved?.Invoke(this, new DriverRemovedEventArgs(driverId, protocolType));
            
            _logger.LogInformation("Driver {DriverId} removed successfully", driverId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove driver {DriverId}", driverId);
            throw;
        }
    }

    public async Task<Dictionary<string, DriverHealthCheckResult>> GetAllDriverHealthAsync(CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, DriverHealthCheckResult>();
        var drivers = await _driverRepository.GetAllAsync(cancellationToken);

        foreach (var driver in drivers)
        {
            var driverId = await GetDriverIdAsync(driver);
            if (driverId != null)
            {
                _healthResults.TryGetValue(driverId, out var healthResult);
                result[driverId] = healthResult ?? DriverHealthCheckResult.CreateUnhealthy("Unknown", "No health data available");
            }
        }

        return result;
    }

    public async Task<Dictionary<string, DriverHealthCheckResult>> PerformHealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, DriverHealthCheckResult>();
        var drivers = await _driverRepository.GetAllAsync(cancellationToken);
        var healthCheckTasks = new List<Task<(string?, DriverHealthCheckResult)>>();

        foreach (var driver in drivers)
        {
            healthCheckTasks.Add(PerformDriverHealthCheckAsync(driver, cancellationToken));
        }

        var healthCheckResults = await Task.WhenAll(healthCheckTasks);
        
        foreach (var (driverId, healthResult) in healthCheckResults)
        {
            if (driverId != null)
            {
                result[driverId] = healthResult;
                
                // Update cached health result
                var previousHealth = _healthResults.TryGetValue(driverId, out var prev) ? prev : null;
                _healthResults.AddOrUpdate(driverId, healthResult, (_, _) => healthResult);
                
                // Raise event if health status changed
                if (previousHealth == null || previousHealth.IsHealthy != healthResult.IsHealthy)
                {
                    DriverHealthChanged?.Invoke(this, new DriverHealthChangedEventArgs(driverId, healthResult));
                }
            }
        }

        return result;
    }

    public async Task<DriverManagerStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await _driverRepository.GetAllAsync(cancellationToken);
        var statusCounts = await _driverRepository.GetCountByStatusAsync(cancellationToken);
        
        var protocolCounts = drivers
            .SelectMany(d => d.Metadata.SupportedProtocols)
            .GroupBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count());

        var healthyCount = _healthResults.Values.Count(h => h.IsHealthy);
        var unhealthyCount = _healthResults.Values.Count(h => !h.IsHealthy);

        return new DriverManagerStatistics(
            drivers.Count(),
            statusCounts,
            protocolCounts,
            healthyCount,
            unhealthyCount);
    }

    private async Task<(string?, DriverHealthCheckResult)> PerformDriverHealthCheckAsync(IDriver driver, CancellationToken cancellationToken)
    {
        try
        {
            var driverId = await GetDriverIdAsync(driver);
            var healthResult = await driver.HealthCheckAsync(cancellationToken);
            return (driverId, healthResult);
        }
        catch (Exception ex)
        {
            var driverId = await GetDriverIdAsync(driver);
            var errorResult = DriverHealthCheckResult.CreateUnhealthy("Error", $"Health check failed: {ex.Message}");
            return (driverId, errorResult);
        }
    }

    private async Task InitializeDriverAsync(IDriver driver, DriverConfiguration configuration, CancellationToken cancellationToken)
    {
        try
        {
            if (driver.Status == DriverStatus.Uninitialized)
            {
                await driver.InitializeAsync(configuration, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            var driverId = await GetDriverIdAsync(driver);
            _logger.LogError(ex, "Failed to initialize driver {DriverId}", driverId);
        }
    }

    private async Task<string?> GetDriverIdAsync(IDriver driver)
    {
        var drivers = await _driverRepository.GetAllAsync();
        return drivers
            .Where(kvp => ReferenceEquals(kvp, driver))
            .Select(kvp => kvp.GetHashCode().ToString())
            .FirstOrDefault();
    }

    private async void PerformPeriodicHealthCheck(object? state)
    {
        if (_disposed) return;

        try
        {
            await PerformHealthCheckAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during periodic health check");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _healthCheckTimer?.Dispose();
        
        try
        {
            ShutdownAllAsync().Wait(TimeSpan.FromSeconds(10));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during driver manager disposal");
        }

        GC.SuppressFinalize(this);
    }
}