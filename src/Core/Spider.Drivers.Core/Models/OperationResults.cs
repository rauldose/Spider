using Spider.Core.SharedKernel.Base;

namespace Spider.Drivers.Core.Models;

/// <summary>
/// Result of driver initialization
/// </summary>
public class DriverInitializationResult : ValueObject
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public Dictionary<string, object> InitializationInfo { get; }
    public TimeSpan InitializationTime { get; }

    private DriverInitializationResult(bool success, string? errorMessage, Dictionary<string, object>? initializationInfo, TimeSpan initializationTime)
    {
        Success = success;
        ErrorMessage = errorMessage;
        InitializationInfo = initializationInfo ?? new Dictionary<string, object>();
        InitializationTime = initializationTime;
    }

    public static DriverInitializationResult CreateSuccess(Dictionary<string, object>? initializationInfo = null, TimeSpan initializationTime = default)
        => new(true, null, initializationInfo, initializationTime);

    public static DriverInitializationResult CreateFailure(string errorMessage, TimeSpan initializationTime = default)
        => new(false, errorMessage, null, initializationTime);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Success;
        yield return ErrorMessage ?? string.Empty;
        yield return InitializationTime;
        foreach (var info in InitializationInfo.OrderBy(x => x.Key))
        {
            yield return info.Key;
            yield return info.Value;
        }
    }
}

/// <summary>
/// Result of driver health check
/// </summary>
public class DriverHealthCheckResult : ValueObject
{
    public bool IsHealthy { get; }
    public string Status { get; }
    public string? ErrorMessage { get; }
    public Dictionary<string, object> HealthMetrics { get; }
    public DateTime CheckedAt { get; }

    private DriverHealthCheckResult(bool isHealthy, string status, string? errorMessage, Dictionary<string, object>? healthMetrics, DateTime checkedAt)
    {
        IsHealthy = isHealthy;
        Status = status;
        ErrorMessage = errorMessage;
        HealthMetrics = healthMetrics ?? new Dictionary<string, object>();
        CheckedAt = checkedAt;
    }

    public static DriverHealthCheckResult CreateHealthy(string status = "Healthy", Dictionary<string, object>? healthMetrics = null)
        => new(true, status, null, healthMetrics, DateTime.UtcNow);

    public static DriverHealthCheckResult CreateUnhealthy(string status, string errorMessage, Dictionary<string, object>? healthMetrics = null)
        => new(false, status, errorMessage, healthMetrics, DateTime.UtcNow);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsHealthy;
        yield return Status;
        yield return ErrorMessage ?? string.Empty;
        yield return CheckedAt;
        foreach (var metric in HealthMetrics.OrderBy(x => x.Key))
        {
            yield return metric.Key;
            yield return metric.Value;
        }
    }
}

/// <summary>
/// Request for reading data
/// </summary>
public class ReadRequest : ValueObject
{
    public string Address { get; }
    public string DataType { get; }
    public int? Length { get; }
    public Dictionary<string, object> Parameters { get; }

    public ReadRequest(string address, string dataType, int? length = null, Dictionary<string, object>? parameters = null)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        Length = length;
        Parameters = parameters ?? new Dictionary<string, object>();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
        yield return DataType;
        yield return Length ?? 0;
        foreach (var parameter in Parameters.OrderBy(x => x.Key))
        {
            yield return parameter.Key;
            yield return parameter.Value;
        }
    }
}

/// <summary>
/// Result of read operation
/// </summary>
public class ReadOperationResult : ValueObject
{
    public bool Success { get; }
    public object? Value { get; }
    public string? ErrorMessage { get; }
    public DateTime Timestamp { get; }
    public string DataQuality { get; }

    private ReadOperationResult(bool success, object? value, string? errorMessage, DateTime timestamp, string dataQuality)
    {
        Success = success;
        Value = value;
        ErrorMessage = errorMessage;
        Timestamp = timestamp;
        DataQuality = dataQuality;
    }

    public static ReadOperationResult CreateSuccess(object value, string dataQuality = "Good")
        => new(true, value, null, DateTime.UtcNow, dataQuality);

    public static ReadOperationResult CreateFailure(string errorMessage)
        => new(false, null, errorMessage, DateTime.UtcNow, "Bad");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Success;
        yield return Value ?? string.Empty;
        yield return ErrorMessage ?? string.Empty;
        yield return Timestamp;
        yield return DataQuality;
    }
}

/// <summary>
/// Result of bulk read operation
/// </summary>
public class BulkReadOperationResult : ValueObject
{
    public bool Success { get; }
    public IReadOnlyDictionary<string, ReadOperationResult> Results { get; }
    public string? ErrorMessage { get; }
    public DateTime Timestamp { get; }

    private BulkReadOperationResult(bool success, Dictionary<string, ReadOperationResult> results, string? errorMessage, DateTime timestamp)
    {
        Success = success;
        Results = results.AsReadOnly();
        ErrorMessage = errorMessage;
        Timestamp = timestamp;
    }

    public static BulkReadOperationResult CreateSuccess(Dictionary<string, ReadOperationResult> results)
        => new(true, results, null, DateTime.UtcNow);

    public static BulkReadOperationResult CreateFailure(string errorMessage)
        => new(false, new Dictionary<string, ReadOperationResult>(), errorMessage, DateTime.UtcNow);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Success;
        yield return ErrorMessage ?? string.Empty;
        yield return Timestamp;
        foreach (var result in Results.OrderBy(x => x.Key))
        {
            yield return result.Key;
            yield return result.Value;
        }
    }
}

/// <summary>
/// Request for writing data
/// </summary>
public class WriteRequest : ValueObject
{
    public string Address { get; }
    public object Value { get; }
    public string DataType { get; }
    public Dictionary<string, object> Parameters { get; }

    public WriteRequest(string address, object value, string dataType, Dictionary<string, object>? parameters = null)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        Parameters = parameters ?? new Dictionary<string, object>();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
        yield return Value;
        yield return DataType;
        foreach (var parameter in Parameters.OrderBy(x => x.Key))
        {
            yield return parameter.Key;
            yield return parameter.Value;
        }
    }
}

/// <summary>
/// Result of write operation
/// </summary>
public class WriteOperationResult : ValueObject
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public DateTime Timestamp { get; }

    private WriteOperationResult(bool success, string? errorMessage, DateTime timestamp)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Timestamp = timestamp;
    }

    public static WriteOperationResult CreateSuccess()
        => new(true, null, DateTime.UtcNow);

    public static WriteOperationResult CreateFailure(string errorMessage)
        => new(false, errorMessage, DateTime.UtcNow);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Success;
        yield return ErrorMessage ?? string.Empty;
        yield return Timestamp;
    }
}

/// <summary>
/// Result of bulk write operation
/// </summary>
public class BulkWriteOperationResult : ValueObject
{
    public bool Success { get; }
    public IReadOnlyDictionary<string, WriteOperationResult> Results { get; }
    public string? ErrorMessage { get; }
    public DateTime Timestamp { get; }

    private BulkWriteOperationResult(bool success, Dictionary<string, WriteOperationResult> results, string? errorMessage, DateTime timestamp)
    {
        Success = success;
        Results = results.AsReadOnly();
        ErrorMessage = errorMessage;
        Timestamp = timestamp;
    }

    public static BulkWriteOperationResult CreateSuccess(Dictionary<string, WriteOperationResult> results)
        => new(true, results, null, DateTime.UtcNow);

    public static BulkWriteOperationResult CreateFailure(string errorMessage)
        => new(false, new Dictionary<string, WriteOperationResult>(), errorMessage, DateTime.UtcNow);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Success;
        yield return ErrorMessage ?? string.Empty;
        yield return Timestamp;
        foreach (var result in Results.OrderBy(x => x.Key))
        {
            yield return result.Key;
            yield return result.Value;
        }
    }
}