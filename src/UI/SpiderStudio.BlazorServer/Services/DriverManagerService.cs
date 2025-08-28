using Spider.Drivers.Core.Abstractions;
using Spider.Drivers.Core.Models;

namespace SpiderStudio.BlazorServer.Services;

/// <summary>
/// Service for managing DDD drivers in the Blazor UI
/// </summary>
public class DriverManagerService
{
    private readonly List<IDriver> _drivers = new();
    private readonly ILogger<DriverManagerService> _logger;

    public event EventHandler? DriversChanged;

    public DriverManagerService(ILogger<DriverManagerService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all registered drivers
    /// </summary>
    public IReadOnlyList<IDriver> GetDrivers() => _drivers.AsReadOnly();

    /// <summary>
    /// Add a new driver
    /// </summary>
    public void AddDriver(IDriver driver)
    {
        if (_drivers.Any(d => d.Metadata.Name == driver.Metadata.Name))
        {
            throw new InvalidOperationException($"Driver with name '{driver.Metadata.Name}' already exists");
        }

        _drivers.Add(driver);
        _logger.LogInformation("Added driver: {DriverName} (Version: {Version})", driver.Metadata.Name, driver.Metadata.Version);
        DriversChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Remove a driver
    /// </summary>
    public bool RemoveDriver(string driverName)
    {
        var driver = _drivers.FirstOrDefault(d => d.Metadata.Name == driverName);
        if (driver != null)
        {
            _drivers.Remove(driver);
            _logger.LogInformation("Removed driver: {DriverName}", driverName);
            DriversChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get driver by name
    /// </summary>
    public IDriver? GetDriverByName(string driverName)
    {
        return _drivers.FirstOrDefault(d => d.Metadata.Name == driverName);
    }

    /// <summary>
    /// Get drivers by protocol type
    /// </summary>
    public IEnumerable<IDriver> GetDriversByProtocol(string protocolType)
    {
        return _drivers.Where(d => d.Metadata.SupportedProtocols.Contains(protocolType, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get available protocol types
    /// </summary>
    public IEnumerable<string> GetAvailableProtocols()
    {
        return _drivers.SelectMany(d => d.Metadata.SupportedProtocols).Distinct().OrderBy(p => p);
    }

    /// <summary>
    /// Connect all drivers
    /// </summary>
    public async Task<Dictionary<string, bool>> ConnectAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>();
        
        foreach (var driver in _drivers)
        {
            try
            {
                // For demo purposes, using empty configuration
                var configuration = new DriverConfiguration("demo://localhost");
                var result = await driver.InitializeAsync(configuration, cancellationToken);
                results[driver.Metadata.Name] = result.Success;
                
                _logger.LogInformation("Driver {DriverName} initialization result: {Success}", driver.Metadata.Name, result.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize driver {DriverName}", driver.Metadata.Name);
                results[driver.Metadata.Name] = false;
            }
        }

        return results;
    }

    /// <summary>
    /// Shutdown all drivers
    /// </summary>
    public async Task ShutdownAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _drivers.Select(driver => ShutdownDriverSafelyAsync(driver, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task ShutdownDriverSafelyAsync(IDriver driver, CancellationToken cancellationToken)
    {
        try
        {
            await driver.ShutdownAsync(cancellationToken);
            _logger.LogInformation("Shutdown driver: {DriverName}", driver.Metadata.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to shutdown driver {DriverName}", driver.Metadata.Name);
        }
    }
}