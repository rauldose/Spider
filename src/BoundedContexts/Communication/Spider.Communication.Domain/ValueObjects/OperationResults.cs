using Spider.Core.SharedKernel.Base;

namespace Spider.Communication.Domain.ValueObjects;

/// <summary>
/// Result of link operations
/// </summary>
public class LinkOperationResult : ValueObject
{
    public bool Success { get; }
    public string? Message { get; }
    public string? ErrorCode { get; }
    public DateTime Timestamp { get; }
    public Dictionary<string, object> Data { get; }

    private LinkOperationResult(bool success, string? message, string? errorCode, DateTime timestamp, Dictionary<string, object>? data)
    {
        Success = success;
        Message = message;
        ErrorCode = errorCode;
        Timestamp = timestamp;
        Data = data ?? new Dictionary<string, object>();
    }

    public static LinkOperationResult CreateSuccess(string? message = null, Dictionary<string, object>? data = null) =>
        new(true, message, null, DateTime.UtcNow, data);

    public static LinkOperationResult CreateFailure(string message, string? errorCode = null, Dictionary<string, object>? data = null) =>
        new(false, message, errorCode, DateTime.UtcNow, data);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Success;
        yield return Message ?? string.Empty;
        yield return ErrorCode ?? string.Empty;
        yield return Timestamp;
        foreach (var item in Data.OrderBy(x => x.Key))
        {
            yield return item.Key;
            yield return item.Value;
        }
    }
}

/// <summary>
/// Result of link health check
/// </summary>
public class LinkHealthResult : ValueObject
{
    public bool IsHealthy { get; }
    public string Status { get; }
    public string? Message { get; }
    public DateTime Timestamp { get; }
    public Dictionary<string, object> Metrics { get; }

    private LinkHealthResult(bool isHealthy, string status, string? message, DateTime timestamp, Dictionary<string, object>? metrics)
    {
        IsHealthy = isHealthy;
        Status = status;
        Message = message;
        Timestamp = timestamp;
        Metrics = metrics ?? new Dictionary<string, object>();
    }

    public static LinkHealthResult CreateHealthy(string? message = null, Dictionary<string, object>? metrics = null) =>
        new(true, "Healthy", message, DateTime.UtcNow, metrics);

    public static LinkHealthResult CreateUnhealthy(string message, Dictionary<string, object>? metrics = null) =>
        new(false, "Unhealthy", message, DateTime.UtcNow, metrics);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsHealthy;
        yield return Status;
        yield return Message ?? string.Empty;
        yield return Timestamp;
        foreach (var metric in Metrics.OrderBy(x => x.Key))
        {
            yield return metric.Key;
            yield return metric.Value;
        }
    }
}

/// <summary>
/// Result of channel read operations
/// </summary>
public class ChannelReadResult : ValueObject
{
    public bool Success { get; }
    public object? Value { get; }
    public string? DataQuality { get; }
    public DateTime? Timestamp { get; }
    public string? ErrorMessage { get; }

    private ChannelReadResult(bool success, object? value, string? dataQuality, DateTime? timestamp, string? errorMessage)
    {
        Success = success;
        Value = value;
        DataQuality = dataQuality;
        Timestamp = timestamp;
        ErrorMessage = errorMessage;
    }

    public static ChannelReadResult CreateSuccess(object value, string dataQuality = "Good", DateTime? timestamp = null) =>
        new(true, value, dataQuality, timestamp ?? DateTime.UtcNow, null);

    public static ChannelReadResult CreateFailure(string errorMessage) =>
        new(false, null, "Bad", DateTime.UtcNow, errorMessage);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Success;
        yield return Value ?? string.Empty;
        yield return DataQuality ?? string.Empty;
        yield return Timestamp ?? DateTime.MinValue;
        yield return ErrorMessage ?? string.Empty;
    }
}

/// <summary>
/// Result of channel write operations
/// </summary>
public class ChannelWriteResult : ValueObject
{
    public bool Success { get; }
    public DateTime? Timestamp { get; }
    public string? ErrorMessage { get; }

    private ChannelWriteResult(bool success, DateTime? timestamp, string? errorMessage)
    {
        Success = success;
        Timestamp = timestamp;
        ErrorMessage = errorMessage;
    }

    public static ChannelWriteResult CreateSuccess(DateTime? timestamp = null) =>
        new(true, timestamp ?? DateTime.UtcNow, null);

    public static ChannelWriteResult CreateFailure(string errorMessage) =>
        new(false, DateTime.UtcNow, errorMessage);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Success;
        yield return Timestamp ?? DateTime.MinValue;
        yield return ErrorMessage ?? string.Empty;
    }
}

/// <summary>
/// Result of channel health check
/// </summary>
public class ChannelHealthResult : ValueObject
{
    public bool IsHealthy { get; }
    public string Status { get; }
    public string? Message { get; }
    public DateTime Timestamp { get; }
    public Dictionary<string, object> Metrics { get; }

    private ChannelHealthResult(bool isHealthy, string status, string? message, DateTime timestamp, Dictionary<string, object>? metrics)
    {
        IsHealthy = isHealthy;
        Status = status;
        Message = message;
        Timestamp = timestamp;
        Metrics = metrics ?? new Dictionary<string, object>();
    }

    public static ChannelHealthResult CreateHealthy(string? message = null, Dictionary<string, object>? metrics = null) =>
        new(true, "Healthy", message, DateTime.UtcNow, metrics);

    public static ChannelHealthResult CreateUnhealthy(string message, Dictionary<string, object>? metrics = null) =>
        new(false, "Unhealthy", message, DateTime.UtcNow, metrics);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsHealthy;
        yield return Status;
        yield return Message ?? string.Empty;
        yield return Timestamp;
        foreach (var metric in Metrics.OrderBy(x => x.Key))
        {
            yield return metric.Key;
            yield return metric.Value;
        }
    }
}