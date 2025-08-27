using SpiderDriver.UnifiedAPI.Interfaces;
using SpiderDriver.UnifiedAPI.Models;

namespace SpiderStudio.BlazorServer.Services;

/// <summary>
/// Service for managing unified drivers in the Blazor UI
/// </summary>
public class DriverManagerService
{
    private readonly List<IUnifiedDriver> _drivers = new();
    private readonly ILogger<DriverManagerService> _logger;

    public event EventHandler? DriversChanged;

    public DriverManagerService(ILogger<DriverManagerService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all registered drivers
    /// </summary>
    public IReadOnlyList<IUnifiedDriver> GetDrivers() => _drivers.AsReadOnly();

    /// <summary>
    /// Add a new driver
    /// </summary>
    public void AddDriver(IUnifiedDriver driver)
    {
        if (_drivers.Any(d => d.Id == driver.Id))
        {
            throw new InvalidOperationException($"Driver with ID '{driver.Id}' already exists");
        }

        _drivers.Add(driver);
        _logger.LogInformation("Added driver: {DriverId} ({ProtocolType})", driver.Id, driver.ProtocolType);
        DriversChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Remove a driver
    /// </summary>
    public bool RemoveDriver(string driverId)
    {
        var driver = _drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver != null)
        {
            _drivers.Remove(driver);
            _logger.LogInformation("Removed driver: {DriverId}", driverId);
            DriversChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get driver by ID
    /// </summary>
    public IUnifiedDriver? GetDriverById(string driverId)
    {
        return _drivers.FirstOrDefault(d => d.Id == driverId);
    }

    /// <summary>
    /// Get drivers by protocol type
    /// </summary>
    public IEnumerable<IUnifiedDriver> GetDriversByProtocol(string protocolType)
    {
        return _drivers.Where(d => d.ProtocolType.Equals(protocolType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get available protocol types
    /// </summary>
    public IEnumerable<string> GetAvailableProtocols()
    {
        return _drivers.Select(d => d.ProtocolType).Distinct().OrderBy(p => p);
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
                // For demo purposes, using empty connection parameters
                var parameters = new ConnectionParameters();
                var success = await driver.ConnectAsync(parameters, cancellationToken);
                results[driver.Id] = success;
                
                _logger.LogInformation("Driver {DriverId} connection result: {Success}", driver.Id, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect driver {DriverId}", driver.Id);
                results[driver.Id] = false;
            }
        }

        return results;
    }

    /// <summary>
    /// Disconnect all drivers
    /// </summary>
    public async Task DisconnectAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _drivers.Select(driver => DisconnectDriverSafelyAsync(driver, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task DisconnectDriverSafelyAsync(IUnifiedDriver driver, CancellationToken cancellationToken)
    {
        try
        {
            await driver.DisconnectAsync(cancellationToken);
            _logger.LogInformation("Disconnected driver: {DriverId}", driver.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect driver {DriverId}", driver.Id);
        }
    }
}