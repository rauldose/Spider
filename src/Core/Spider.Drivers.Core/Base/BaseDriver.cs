using Microsoft.Extensions.Logging;
using Spider.Drivers.Core.Abstractions;
using Spider.Drivers.Core.Models;

namespace Spider.Drivers.Core.Base;

/// <summary>
/// Base implementation for all drivers providing common functionality
/// </summary>
public abstract class BaseDriver : IDriver, IDiagnosticDriver
{
    protected readonly ILogger _logger;
    private readonly PerformanceTracker _performanceTracker;
    private DriverStatus _status = DriverStatus.Uninitialized;
    private bool _disposed;
    private bool _verboseLogging;

    protected BaseDriver(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _performanceTracker = new PerformanceTracker();
    }

    /// <summary>
    /// Driver metadata - must be implemented by derived classes
    /// </summary>
    public abstract DriverMetadata Metadata { get; }

    /// <summary>
    /// Driver capabilities - must be implemented by derived classes
    /// </summary>
    public abstract DriverCapabilities Capabilities { get; }

    /// <summary>
    /// Current driver status
    /// </summary>
    public DriverStatus Status 
    { 
        get => _status;
        protected set 
        {
            if (_status != value)
            {
                var previousStatus = _status;
                _status = value;
                OnStatusChanged(previousStatus, value);
            }
        }
    }

    /// <summary>
    /// Event raised when driver status changes
    /// </summary>
    public event EventHandler<DriverStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Event raised when an error occurs
    /// </summary>
    public event EventHandler<DriverErrorEventArgs>? ErrorOccurred;

    /// <summary>
    /// Initialize the driver
    /// </summary>
    public virtual async Task<DriverInitializationResult> InitializeAsync(DriverConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            Status = DriverStatus.Initializing;
            _logger.LogInformation("Initializing driver {DriverName} v{Version}", Metadata.Name, Metadata.Version);

            var startTime = DateTime.UtcNow;
            var initResult = await InitializeDriverAsync(configuration, cancellationToken);
            var initTime = DateTime.UtcNow - startTime;

            if (initResult.Success)
            {
                Status = DriverStatus.Ready;
                _logger.LogInformation("Driver {DriverName} initialized successfully in {InitTime}ms", 
                    Metadata.Name, initTime.TotalMilliseconds);
            }
            else
            {
                Status = DriverStatus.Error;
                _logger.LogError("Driver {DriverName} initialization failed: {Error}", 
                    Metadata.Name, initResult.ErrorMessage);
            }

            return initResult;
        }
        catch (Exception ex)
        {
            Status = DriverStatus.Error;
            var errorMessage = $"Driver initialization failed: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            OnErrorOccurred(ex, "INIT_ERROR", errorMessage);
            return DriverInitializationResult.CreateFailure(errorMessage);
        }
    }

    /// <summary>
    /// Perform health check
    /// </summary>
    public virtual async Task<DriverHealthCheckResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_disposed)
            {
                return DriverHealthCheckResult.CreateUnhealthy("Disposed", "Driver has been disposed");
            }

            if (Status == DriverStatus.Error)
            {
                return DriverHealthCheckResult.CreateUnhealthy("Error", "Driver is in error state");
            }

            var healthResult = await PerformHealthCheckAsync(cancellationToken);
            
            if (_verboseLogging)
            {
                _logger.LogDebug("Health check for {DriverName}: {Status}", Metadata.Name, 
                    healthResult.IsHealthy ? "Healthy" : "Unhealthy");
            }

            return healthResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for driver {DriverName}", Metadata.Name);
            OnErrorOccurred(ex, "HEALTH_CHECK_ERROR", "Health check failed");
            return DriverHealthCheckResult.CreateUnhealthy("Error", ex.Message);
        }
    }

    /// <summary>
    /// Shutdown the driver
    /// </summary>
    public virtual async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_disposed || Status == DriverStatus.Shutdown)
                return;

            _logger.LogInformation("Shutting down driver {DriverName}", Metadata.Name);
            Status = DriverStatus.Shutdown;

            await ShutdownDriverAsync(cancellationToken);
            
            _logger.LogInformation("Driver {DriverName} shutdown completed", Metadata.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during driver shutdown for {DriverName}", Metadata.Name);
            OnErrorOccurred(ex, "SHUTDOWN_ERROR", "Shutdown failed");
        }
    }

    /// <summary>
    /// Get diagnostic information
    /// </summary>
    public virtual async Task<DiagnosticInfo> GetDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var systemInfo = new Dictionary<string, object>
        {
            { "Status", Status.Name },
            { "Disposed", _disposed },
            { "VerboseLogging", _verboseLogging },
            { "LastActivity", _performanceTracker.LastOperationTime }
        };

        var customDiagnostics = await GetCustomDiagnosticsAsync(cancellationToken);

        return new DiagnosticInfo(
            Metadata.Name,
            Status,
            _performanceTracker.LastOperationTime,
            systemInfo,
            await GetConnectionInfoAsync(cancellationToken),
            _performanceTracker.GetRecentErrors(),
            customDiagnostics);
    }

    /// <summary>
    /// Get performance metrics
    /// </summary>
    public virtual Task<PerformanceMetrics> GetPerformanceMetricsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_performanceTracker.GetMetrics());
    }

    /// <summary>
    /// Reset performance counters
    /// </summary>
    public virtual Task ResetPerformanceCountersAsync(CancellationToken cancellationToken = default)
    {
        _performanceTracker.Reset();
        _logger.LogInformation("Performance counters reset for driver {DriverName}", Metadata.Name);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Enable or disable verbose logging
    /// </summary>
    public virtual void SetVerboseLogging(bool enabled)
    {
        _verboseLogging = enabled;
        _logger.LogInformation("Verbose logging {Status} for driver {DriverName}", 
            enabled ? "enabled" : "disabled", Metadata.Name);
    }

    /// <summary>
    /// Dispose the driver
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Status = DriverStatus.Shutdown;

        try
        {
            DisposeDriver();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during driver disposal for {DriverName}", Metadata.Name);
        }

        GC.SuppressFinalize(this);
    }

    #region Protected Methods

    /// <summary>
    /// Track operation performance
    /// </summary>
    protected void TrackOperation(bool success, TimeSpan duration)
    {
        _performanceTracker.RecordOperation(success, duration);
    }

    /// <summary>
    /// Track operation error
    /// </summary>
    protected void TrackError(string error)
    {
        _performanceTracker.RecordError(error);
    }

    /// <summary>
    /// Raise status changed event
    /// </summary>
    protected virtual void OnStatusChanged(DriverStatus previousStatus, DriverStatus currentStatus)
    {
        StatusChanged?.Invoke(this, new DriverStatusChangedEventArgs(previousStatus, currentStatus));
    }

    /// <summary>
    /// Raise error occurred event
    /// </summary>
    protected virtual void OnErrorOccurred(Exception exception, string errorCode, string? errorMessage = null, Dictionary<string, object>? context = null)
    {
        TrackError(errorMessage ?? exception.Message);
        ErrorOccurred?.Invoke(this, new DriverErrorEventArgs(exception, errorCode, errorMessage, context));
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Initialize the specific driver implementation
    /// </summary>
    protected abstract Task<DriverInitializationResult> InitializeDriverAsync(DriverConfiguration configuration, CancellationToken cancellationToken);

    /// <summary>
    /// Perform driver-specific health check
    /// </summary>
    protected abstract Task<DriverHealthCheckResult> PerformHealthCheckAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Shutdown the specific driver implementation
    /// </summary>
    protected abstract Task ShutdownDriverAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Dispose driver-specific resources
    /// </summary>
    protected abstract void DisposeDriver();

    #endregion

    #region Virtual Methods

    /// <summary>
    /// Get custom diagnostic information specific to the driver
    /// </summary>
    protected virtual Task<Dictionary<string, object>> GetCustomDiagnosticsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new Dictionary<string, object>());
    }

    /// <summary>
    /// Get connection information for diagnostics
    /// </summary>
    protected virtual Task<Dictionary<string, object>> GetConnectionInfoAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new Dictionary<string, object>());
    }

    #endregion
}

/// <summary>
/// Internal class to track performance metrics
/// </summary>
internal class PerformanceTracker
{
    private long _totalOperations;
    private long _successfulOperations;
    private long _failedOperations;
    private readonly List<double> _latencies = new();
    private readonly List<string> _recentErrors = new();
    private readonly DateTime _startTime = DateTime.UtcNow;
    private DateTime _lastResetTime = DateTime.UtcNow;
    private DateTime _lastOperationTime = DateTime.UtcNow;
    private readonly object _lock = new();

    public DateTime LastOperationTime => _lastOperationTime;

    public void RecordOperation(bool success, TimeSpan duration)
    {
        lock (_lock)
        {
            _totalOperations++;
            _lastOperationTime = DateTime.UtcNow;
            
            if (success)
                _successfulOperations++;
            else
                _failedOperations++;

            _latencies.Add(duration.TotalMilliseconds);
            
            // Keep only the last 1000 latencies to prevent memory bloat
            if (_latencies.Count > 1000)
                _latencies.RemoveAt(0);
        }
    }

    public void RecordError(string error)
    {
        lock (_lock)
        {
            _recentErrors.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}: {error}");
            
            // Keep only the last 50 errors
            if (_recentErrors.Count > 50)
                _recentErrors.RemoveAt(0);
        }
    }

    public PerformanceMetrics GetMetrics()
    {
        lock (_lock)
        {
            return new PerformanceMetrics(
                _totalOperations,
                _successfulOperations,
                _failedOperations,
                _latencies.Count > 0 ? _latencies.Average() : 0,
                _latencies.Count > 0 ? _latencies.Min() : 0,
                _latencies.Count > 0 ? _latencies.Max() : 0,
                _startTime,
                _lastResetTime);
        }
    }

    public List<string> GetRecentErrors()
    {
        lock (_lock)
        {
            return new List<string>(_recentErrors);
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _totalOperations = 0;
            _successfulOperations = 0;
            _failedOperations = 0;
            _latencies.Clear();
            _recentErrors.Clear();
            _lastResetTime = DateTime.UtcNow;
        }
    }
}