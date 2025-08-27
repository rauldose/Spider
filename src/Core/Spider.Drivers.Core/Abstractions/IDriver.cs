using Spider.Drivers.Core.Models;

namespace Spider.Drivers.Core.Abstractions;

/// <summary>
/// Base interface for all drivers in the Spider IoT platform
/// </summary>
public interface IDriver : IDisposable
{
    /// <summary>
    /// Driver metadata containing information about the driver
    /// </summary>
    DriverMetadata Metadata { get; }
    
    /// <summary>
    /// Current status of the driver
    /// </summary>
    DriverStatus Status { get; }
    
    /// <summary>
    /// Capabilities supported by this driver
    /// </summary>
    DriverCapabilities Capabilities { get; }
    
    /// <summary>
    /// Initialize the driver with the specified configuration
    /// </summary>
    Task<DriverInitializationResult> InitializeAsync(DriverConfiguration configuration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Perform a health check on the driver
    /// </summary>
    Task<DriverHealthCheckResult> HealthCheckAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gracefully shutdown the driver
    /// </summary>
    Task ShutdownAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when driver status changes
    /// </summary>
    event EventHandler<DriverStatusChangedEventArgs> StatusChanged;
    
    /// <summary>
    /// Event raised when an error occurs in the driver
    /// </summary>
    event EventHandler<DriverErrorEventArgs> ErrorOccurred;
}

/// <summary>
/// Interface for drivers that support reading operations
/// </summary>
public interface IReadableDriver : IDriver
{
    /// <summary>
    /// Read data from a single address
    /// </summary>
    Task<ReadOperationResult> ReadAsync(ReadRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Read data from multiple addresses in a single operation
    /// </summary>
    Task<BulkReadOperationResult> BulkReadAsync(IEnumerable<ReadRequest> requests, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Indicates if the driver supports asynchronous reading
    /// </summary>
    bool SupportsAsyncReading { get; }
}

/// <summary>
/// Interface for drivers that support writing operations
/// </summary>
public interface IWritableDriver : IDriver
{
    /// <summary>
    /// Write data to a single address
    /// </summary>
    Task<WriteOperationResult> WriteAsync(WriteRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Write data to multiple addresses in a single operation
    /// </summary>
    Task<BulkWriteOperationResult> BulkWriteAsync(IEnumerable<WriteRequest> requests, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Indicates if the driver supports asynchronous writing
    /// </summary>
    bool SupportsAsyncWriting { get; }
}

/// <summary>
/// Interface for drivers that support subscription-based data monitoring
/// </summary>
public interface ISubscribableDriver : IDriver
{
    /// <summary>
    /// Subscribe to data changes for the specified addresses
    /// </summary>
    Task<SubscriptionResult> SubscribeAsync(SubscriptionRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unsubscribe from data changes for the specified subscription
    /// </summary>
    Task<UnsubscriptionResult> UnsubscribeAsync(string subscriptionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when subscribed data changes
    /// </summary>
    event EventHandler<DataChangedEventArgs> DataChanged;
    
    /// <summary>
    /// Maximum number of concurrent subscriptions supported
    /// </summary>
    int MaxSubscriptions { get; }
}

/// <summary>
/// Interface for drivers that support real-time communication
/// </summary>
public interface IRealTimeDriver : IDriver
{
    /// <summary>
    /// Current latency of the driver connection
    /// </summary>
    TimeSpan CurrentLatency { get; }
    
    /// <summary>
    /// Maximum acceptable latency for real-time operations
    /// </summary>
    TimeSpan MaxLatency { get; }
    
    /// <summary>
    /// Enable real-time mode with specified parameters
    /// </summary>
    Task<RealTimeResult> EnableRealTimeModeAsync(RealTimeConfiguration config, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Disable real-time mode
    /// </summary>
    Task DisableRealTimeModeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when latency threshold is exceeded
    /// </summary>
    event EventHandler<LatencyThresholdExceededEventArgs> LatencyThresholdExceeded;
}

/// <summary>
/// Interface for drivers that support diagnostics and monitoring
/// </summary>
public interface IDiagnosticDriver : IDriver
{
    /// <summary>
    /// Get comprehensive diagnostic information
    /// </summary>
    Task<DiagnosticInfo> GetDiagnosticsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get performance metrics for the driver
    /// </summary>
    Task<PerformanceMetrics> GetPerformanceMetricsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reset performance counters
    /// </summary>
    Task ResetPerformanceCountersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Enable or disable verbose logging
    /// </summary>
    void SetVerboseLogging(bool enabled);
}