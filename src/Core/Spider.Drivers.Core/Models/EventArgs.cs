namespace Spider.Drivers.Core.Models;

/// <summary>
/// Event arguments for driver status changes
/// </summary>
public class DriverStatusChangedEventArgs : EventArgs
{
    public DriverStatus PreviousStatus { get; }
    public DriverStatus CurrentStatus { get; }
    public string? Reason { get; }
    public DateTime Timestamp { get; }

    public DriverStatusChangedEventArgs(DriverStatus previousStatus, DriverStatus currentStatus, string? reason = null)
    {
        PreviousStatus = previousStatus;
        CurrentStatus = currentStatus;
        Reason = reason;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Event arguments for driver errors
/// </summary>
public class DriverErrorEventArgs : EventArgs
{
    public Exception Exception { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }
    public DateTime Timestamp { get; }
    public Dictionary<string, object> Context { get; }

    public DriverErrorEventArgs(Exception exception, string errorCode, string? errorMessage = null, Dictionary<string, object>? context = null)
    {
        Exception = exception;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage ?? exception.Message;
        Timestamp = DateTime.UtcNow;
        Context = context ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Request for subscription to data changes
/// </summary>
public class SubscriptionRequest
{
    public string SubscriptionId { get; }
    public IReadOnlyList<string> Addresses { get; }
    public TimeSpan UpdateInterval { get; }
    public Dictionary<string, object> Parameters { get; }

    public SubscriptionRequest(string subscriptionId, IEnumerable<string> addresses, TimeSpan updateInterval, Dictionary<string, object>? parameters = null)
    {
        SubscriptionId = subscriptionId ?? throw new ArgumentNullException(nameof(subscriptionId));
        Addresses = addresses?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(addresses));
        UpdateInterval = updateInterval;
        Parameters = parameters ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Result of subscription operation
/// </summary>
public class SubscriptionResult
{
    public bool Success { get; }
    public string SubscriptionId { get; }
    public string? ErrorMessage { get; }
    public DateTime Timestamp { get; }

    private SubscriptionResult(bool success, string subscriptionId, string? errorMessage, DateTime timestamp)
    {
        Success = success;
        SubscriptionId = subscriptionId;
        ErrorMessage = errorMessage;
        Timestamp = timestamp;
    }

    public static SubscriptionResult CreateSuccess(string subscriptionId)
        => new(true, subscriptionId, null, DateTime.UtcNow);

    public static SubscriptionResult CreateFailure(string subscriptionId, string errorMessage)
        => new(false, subscriptionId, errorMessage, DateTime.UtcNow);
}

/// <summary>
/// Result of unsubscription operation
/// </summary>
public class UnsubscriptionResult
{
    public bool Success { get; }
    public string SubscriptionId { get; }
    public string? ErrorMessage { get; }
    public DateTime Timestamp { get; }

    private UnsubscriptionResult(bool success, string subscriptionId, string? errorMessage, DateTime timestamp)
    {
        Success = success;
        SubscriptionId = subscriptionId;
        ErrorMessage = errorMessage;
        Timestamp = timestamp;
    }

    public static UnsubscriptionResult CreateSuccess(string subscriptionId)
        => new(true, subscriptionId, null, DateTime.UtcNow);

    public static UnsubscriptionResult CreateFailure(string subscriptionId, string errorMessage)
        => new(false, subscriptionId, errorMessage, DateTime.UtcNow);
}

/// <summary>
/// Event arguments for data changes in subscriptions
/// </summary>
public class DataChangedEventArgs : EventArgs
{
    public string SubscriptionId { get; }
    public string Address { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
    public DateTime Timestamp { get; }
    public string DataQuality { get; }

    public DataChangedEventArgs(string subscriptionId, string address, object? oldValue, object? newValue, string dataQuality = "Good")
    {
        SubscriptionId = subscriptionId;
        Address = address;
        OldValue = oldValue;
        NewValue = newValue;
        Timestamp = DateTime.UtcNow;
        DataQuality = dataQuality;
    }
}

/// <summary>
/// Configuration for real-time mode
/// </summary>
public class RealTimeConfiguration
{
    public TimeSpan MaxLatency { get; }
    public int Priority { get; }
    public bool EnablePriorityBoost { get; }
    public Dictionary<string, object> Parameters { get; }

    public RealTimeConfiguration(TimeSpan maxLatency, int priority = 0, bool enablePriorityBoost = false, Dictionary<string, object>? parameters = null)
    {
        MaxLatency = maxLatency;
        Priority = priority;
        EnablePriorityBoost = enablePriorityBoost;
        Parameters = parameters ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Result of real-time mode operation
/// </summary>
public class RealTimeResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public TimeSpan ActualLatency { get; }
    public DateTime Timestamp { get; }

    private RealTimeResult(bool success, string? errorMessage, TimeSpan actualLatency, DateTime timestamp)
    {
        Success = success;
        ErrorMessage = errorMessage;
        ActualLatency = actualLatency;
        Timestamp = timestamp;
    }

    public static RealTimeResult CreateSuccess(TimeSpan actualLatency)
        => new(true, null, actualLatency, DateTime.UtcNow);

    public static RealTimeResult CreateFailure(string errorMessage)
        => new(false, errorMessage, TimeSpan.Zero, DateTime.UtcNow);
}

/// <summary>
/// Event arguments for latency threshold exceeded
/// </summary>
public class LatencyThresholdExceededEventArgs : EventArgs
{
    public TimeSpan CurrentLatency { get; }
    public TimeSpan ThresholdLatency { get; }
    public DateTime Timestamp { get; }

    public LatencyThresholdExceededEventArgs(TimeSpan currentLatency, TimeSpan thresholdLatency)
    {
        CurrentLatency = currentLatency;
        ThresholdLatency = thresholdLatency;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Comprehensive diagnostic information
/// </summary>
public class DiagnosticInfo
{
    public string DriverName { get; }
    public DriverStatus Status { get; }
    public DateTime LastActivity { get; }
    public Dictionary<string, object> SystemInfo { get; }
    public Dictionary<string, object> ConnectionInfo { get; }
    public List<string> RecentErrors { get; }
    public Dictionary<string, object> CustomDiagnostics { get; }

    public DiagnosticInfo(
        string driverName, 
        DriverStatus status, 
        DateTime lastActivity,
        Dictionary<string, object>? systemInfo = null,
        Dictionary<string, object>? connectionInfo = null,
        List<string>? recentErrors = null,
        Dictionary<string, object>? customDiagnostics = null)
    {
        DriverName = driverName;
        Status = status;
        LastActivity = lastActivity;
        SystemInfo = systemInfo ?? new Dictionary<string, object>();
        ConnectionInfo = connectionInfo ?? new Dictionary<string, object>();
        RecentErrors = recentErrors ?? new List<string>();
        CustomDiagnostics = customDiagnostics ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Performance metrics for a driver
/// </summary>
public class PerformanceMetrics
{
    public long TotalOperations { get; }
    public long SuccessfulOperations { get; }
    public long FailedOperations { get; }
    public double AverageLatency { get; }
    public double MinLatency { get; }
    public double MaxLatency { get; }
    public DateTime StartTime { get; }
    public DateTime LastResetTime { get; }
    public Dictionary<string, object> CustomMetrics { get; }

    public PerformanceMetrics(
        long totalOperations,
        long successfulOperations,
        long failedOperations,
        double averageLatency,
        double minLatency,
        double maxLatency,
        DateTime startTime,
        DateTime lastResetTime,
        Dictionary<string, object>? customMetrics = null)
    {
        TotalOperations = totalOperations;
        SuccessfulOperations = successfulOperations;
        FailedOperations = failedOperations;
        AverageLatency = averageLatency;
        MinLatency = minLatency;
        MaxLatency = maxLatency;
        StartTime = startTime;
        LastResetTime = lastResetTime;
        CustomMetrics = customMetrics ?? new Dictionary<string, object>();
    }

    public double SuccessRate => TotalOperations > 0 ? (double)SuccessfulOperations / TotalOperations * 100 : 0;
}