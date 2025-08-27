using Spider.Drivers.Core.Abstractions;
using Spider.Drivers.Core.Models;

namespace Spider.Drivers.Core.Abstractions;

/// <summary>
/// Repository interface for driver management following DDD patterns
/// </summary>
public interface IDriverRepository
{
    /// <summary>
    /// Get a driver by its unique identifier
    /// </summary>
    Task<IDriver?> GetByIdAsync(string driverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all drivers
    /// </summary>
    Task<IEnumerable<IDriver>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get drivers by protocol type
    /// </summary>
    Task<IEnumerable<IDriver>> GetByProtocolAsync(string protocolType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get drivers by status
    /// </summary>
    Task<IEnumerable<IDriver>> GetByStatusAsync(DriverStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new driver to the repository
    /// </summary>
    Task<string> AddAsync(IDriver driver, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing driver
    /// </summary>
    Task UpdateAsync(IDriver driver, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a driver from the repository
    /// </summary>
    Task RemoveAsync(string driverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a driver exists
    /// </summary>
    Task<bool> ExistsAsync(string driverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get driver count by status
    /// </summary>
    Task<Dictionary<DriverStatus, int>> GetCountByStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory interface for creating drivers
/// </summary>
public interface IDriverFactory
{
    /// <summary>
    /// Create a driver for the specified protocol
    /// </summary>
    IDriver CreateDriver(string protocolType, DriverConfiguration? configuration = null);

    /// <summary>
    /// Create a driver of a specific type
    /// </summary>
    T CreateDriver<T>(DriverConfiguration? configuration = null) where T : class, IDriver;

    /// <summary>
    /// Get all supported protocol types
    /// </summary>
    IEnumerable<string> GetSupportedProtocols();

    /// <summary>
    /// Get driver capabilities for a protocol type
    /// </summary>
    DriverCapabilities GetProtocolCapabilities(string protocolType);

    /// <summary>
    /// Check if a protocol is supported
    /// </summary>
    bool IsProtocolSupported(string protocolType);

    /// <summary>
    /// Register a new driver type
    /// </summary>
    void RegisterDriver<T>(string protocolType) where T : class, IDriver;

    /// <summary>
    /// Unregister a driver type
    /// </summary>
    void UnregisterDriver(string protocolType);
}

/// <summary>
/// Manager interface for driver lifecycle management
/// </summary>
public interface IDriverManager
{
    /// <summary>
    /// Initialize all drivers
    /// </summary>
    Task InitializeAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Shutdown all drivers
    /// </summary>
    Task ShutdownAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get driver by ID
    /// </summary>
    Task<IDriver?> GetDriverAsync(string driverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create and register a new driver
    /// </summary>
    Task<string> CreateDriverAsync(string protocolType, DriverConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a driver
    /// </summary>
    Task RemoveDriverAsync(string driverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all drivers with health status
    /// </summary>
    Task<Dictionary<string, DriverHealthCheckResult>> GetAllDriverHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform health check on all drivers
    /// </summary>
    Task<Dictionary<string, DriverHealthCheckResult>> PerformHealthCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get driver statistics
    /// </summary>
    Task<DriverManagerStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when a driver is added
    /// </summary>
    event EventHandler<DriverAddedEventArgs> DriverAdded;

    /// <summary>
    /// Event raised when a driver is removed
    /// </summary>
    event EventHandler<DriverRemovedEventArgs> DriverRemoved;

    /// <summary>
    /// Event raised when driver health status changes
    /// </summary>
    event EventHandler<DriverHealthChangedEventArgs> DriverHealthChanged;
}

/// <summary>
/// Statistics for driver manager
/// </summary>
public class DriverManagerStatistics
{
    public int TotalDrivers { get; }
    public Dictionary<DriverStatus, int> DriversByStatus { get; }
    public Dictionary<string, int> DriversByProtocol { get; }
    public int HealthyDrivers { get; }
    public int UnhealthyDrivers { get; }
    public DateTime LastUpdated { get; }

    public DriverManagerStatistics(
        int totalDrivers,
        Dictionary<DriverStatus, int> driversByStatus,
        Dictionary<string, int> driversByProtocol,
        int healthyDrivers,
        int unhealthyDrivers)
    {
        TotalDrivers = totalDrivers;
        DriversByStatus = driversByStatus;
        DriversByProtocol = driversByProtocol;
        HealthyDrivers = healthyDrivers;
        UnhealthyDrivers = unhealthyDrivers;
        LastUpdated = DateTime.UtcNow;
    }
}

/// <summary>
/// Event arguments for driver added event
/// </summary>
public class DriverAddedEventArgs : EventArgs
{
    public string DriverId { get; }
    public string ProtocolType { get; }
    public DateTime Timestamp { get; }

    public DriverAddedEventArgs(string driverId, string protocolType)
    {
        DriverId = driverId;
        ProtocolType = protocolType;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Event arguments for driver removed event
/// </summary>
public class DriverRemovedEventArgs : EventArgs
{
    public string DriverId { get; }
    public string ProtocolType { get; }
    public DateTime Timestamp { get; }

    public DriverRemovedEventArgs(string driverId, string protocolType)
    {
        DriverId = driverId;
        ProtocolType = protocolType;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Event arguments for driver health changed event
/// </summary>
public class DriverHealthChangedEventArgs : EventArgs
{
    public string DriverId { get; }
    public DriverHealthCheckResult HealthResult { get; }
    public DateTime Timestamp { get; }

    public DriverHealthChangedEventArgs(string driverId, DriverHealthCheckResult healthResult)
    {
        DriverId = driverId;
        HealthResult = healthResult;
        Timestamp = DateTime.UtcNow;
    }
}