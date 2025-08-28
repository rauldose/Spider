using Spider.Core.SharedKernel.Base;

namespace Spider.Communication.Domain.ValueObjects;

/// <summary>
/// Link metadata value object
/// </summary>
public class LinkMetadata : ValueObject
{
    public string Name { get; }
    public string Description { get; }
    public string ProtocolType { get; }
    public string Version { get; }
    public Dictionary<string, string> Tags { get; }

    public LinkMetadata(string name, string description, string protocolType, string version = "1.0.0", Dictionary<string, string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(protocolType))
            throw new ArgumentException("Protocol type cannot be empty", nameof(protocolType));

        Name = name;
        Description = description ?? string.Empty;
        ProtocolType = protocolType;
        Version = version;
        Tags = tags ?? new Dictionary<string, string>();
    }

    // Parameterless constructor for EF Core
    private LinkMetadata()
    {
        Name = "Unknown";
        Description = string.Empty;
        ProtocolType = "Unknown";
        Version = "1.0.0";
        Tags = new Dictionary<string, string>();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Description;
        yield return ProtocolType;
        yield return Version;
        foreach (var tag in Tags.OrderBy(x => x.Key))
        {
            yield return tag.Key;
            yield return tag.Value;
        }
    }
}

/// <summary>
/// Link configuration value object
/// </summary>
public class LinkConfiguration : ValueObject
{
    public string ConnectionString { get; }
    public Dictionary<string, object> Parameters { get; }
    public TimeSpan ConnectionTimeout { get; }
    public TimeSpan OperationTimeout { get; }
    public TimeSpan HealthCheckInterval { get; }
    public int MaxChannels { get; }
    public bool AutoReconnect { get; }
    public int MaxRetryAttempts { get; }

    public LinkConfiguration(
        string connectionString,
        Dictionary<string, object>? parameters = null,
        TimeSpan connectionTimeout = default,
        TimeSpan operationTimeout = default,
        TimeSpan healthCheckInterval = default,
        int maxChannels = 10,
        bool autoReconnect = true,
        int maxRetryAttempts = 3)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        ConnectionString = connectionString;
        Parameters = parameters ?? new Dictionary<string, object>();
        ConnectionTimeout = connectionTimeout == default ? TimeSpan.FromSeconds(30) : connectionTimeout;
        OperationTimeout = operationTimeout == default ? TimeSpan.FromSeconds(10) : operationTimeout;
        HealthCheckInterval = healthCheckInterval == default ? TimeSpan.FromMinutes(1) : healthCheckInterval;
        MaxChannels = maxChannels > 0 ? maxChannels : throw new ArgumentException("Max channels must be positive", nameof(maxChannels));
        AutoReconnect = autoReconnect;
        MaxRetryAttempts = maxRetryAttempts >= 0 ? maxRetryAttempts : throw new ArgumentException("Max retry attempts cannot be negative", nameof(maxRetryAttempts));
    }

    // Parameterless constructor for EF Core
    private LinkConfiguration()
    {
        ConnectionString = "localhost:502";
        Parameters = new Dictionary<string, object>();
        ConnectionTimeout = TimeSpan.FromSeconds(30);
        OperationTimeout = TimeSpan.FromSeconds(10);
        HealthCheckInterval = TimeSpan.FromMinutes(1);
        MaxChannels = 10;
        AutoReconnect = true;
        MaxRetryAttempts = 3;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ConnectionString;
        yield return ConnectionTimeout;
        yield return OperationTimeout;
        yield return HealthCheckInterval;
        yield return MaxChannels;
        yield return AutoReconnect;
        yield return MaxRetryAttempts;
        foreach (var parameter in Parameters.OrderBy(x => x.Key))
        {
            yield return parameter.Key;
            yield return parameter.Value;
        }
    }
}

/// <summary>
/// Link status enumeration
/// </summary>
public class LinkStatus : Enumeration
{
    public static readonly LinkStatus Disconnected = new(0, nameof(Disconnected));
    public static readonly LinkStatus Connecting = new(1, nameof(Connecting));
    public static readonly LinkStatus Connected = new(2, nameof(Connected));
    public static readonly LinkStatus Disconnecting = new(3, nameof(Disconnecting));
    public static readonly LinkStatus Error = new(4, nameof(Error));

    public LinkStatus(int id, string name) : base(id, name) { }
}

/// <summary>
/// Link health value object
/// </summary>
public class LinkHealth : ValueObject
{
    public bool IsHealthy { get; }
    public string Status { get; }
    public string? ErrorMessage { get; }
    public DateTime LastChecked { get; }
    public Dictionary<string, object> Metrics { get; }

    private LinkHealth(bool isHealthy, string status, string? errorMessage, DateTime lastChecked, Dictionary<string, object>? metrics)
    {
        IsHealthy = isHealthy;
        Status = status;
        ErrorMessage = errorMessage;
        LastChecked = lastChecked;
        Metrics = metrics ?? new Dictionary<string, object>();
    }

    // Parameterless constructor for EF Core
    private LinkHealth()
    {
        IsHealthy = false;
        Status = "Unknown";
        ErrorMessage = null;
        LastChecked = DateTime.UtcNow;
        Metrics = new Dictionary<string, object>();
    }

    public static LinkHealth Good(DateTime lastChecked, Dictionary<string, object>? metrics = null) =>
        new(true, "Good", null, lastChecked, metrics);

    public static LinkHealth Bad(string errorMessage, DateTime lastChecked, Dictionary<string, object>? metrics = null) =>
        new(false, "Bad", errorMessage, lastChecked, metrics);

    public static LinkHealth Unknown(Dictionary<string, object>? metrics = null) =>
        new(false, "Unknown", null, DateTime.UtcNow, metrics);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsHealthy;
        yield return Status;
        yield return ErrorMessage ?? string.Empty;
        yield return LastChecked;
        foreach (var metric in Metrics.OrderBy(x => x.Key))
        {
            yield return metric.Key;
            yield return metric.Value;
        }
    }
}

/// <summary>
/// Channel type enumeration
/// </summary>
public class ChannelType : Enumeration
{
    public static readonly ChannelType Input = new(0, nameof(Input));
    public static readonly ChannelType Output = new(1, nameof(Output));
    public static readonly ChannelType Bidirectional = new(2, nameof(Bidirectional));

    public ChannelType(int id, string name) : base(id, name) { }
}

/// <summary>
/// Channel configuration value object
/// </summary>
public class ChannelConfiguration : ValueObject
{
    public TimeSpan ScanInterval { get; }
    public TimeSpan HealthCheckInterval { get; }
    public int MaxDataPoints { get; }
    public bool EnableBuffering { get; }
    public int BufferSize { get; }
    public Dictionary<string, object> Properties { get; }

    public ChannelConfiguration(
        TimeSpan scanInterval = default,
        TimeSpan healthCheckInterval = default,
        int maxDataPoints = 100,
        bool enableBuffering = false,
        int bufferSize = 1000,
        Dictionary<string, object>? properties = null)
    {
        ScanInterval = scanInterval == default ? TimeSpan.FromSeconds(1) : scanInterval;
        HealthCheckInterval = healthCheckInterval == default ? TimeSpan.FromMinutes(5) : healthCheckInterval;
        MaxDataPoints = maxDataPoints > 0 ? maxDataPoints : throw new ArgumentException("Max data points must be positive", nameof(maxDataPoints));
        EnableBuffering = enableBuffering;
        BufferSize = bufferSize > 0 ? bufferSize : throw new ArgumentException("Buffer size must be positive", nameof(bufferSize));
        Properties = properties ?? new Dictionary<string, object>();
    }

    // Parameterless constructor for EF Core
    private ChannelConfiguration()
    {
        ScanInterval = TimeSpan.FromSeconds(1);
        HealthCheckInterval = TimeSpan.FromMinutes(5);
        MaxDataPoints = 100;
        EnableBuffering = false;
        BufferSize = 1000;
        Properties = new Dictionary<string, object>();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ScanInterval;
        yield return HealthCheckInterval;
        yield return MaxDataPoints;
        yield return EnableBuffering;
        yield return BufferSize;
        foreach (var property in Properties.OrderBy(x => x.Key))
        {
            yield return property.Key;
            yield return property.Value;
        }
    }
}

/// <summary>
/// Channel status enumeration
/// </summary>
public class ChannelStatus : Enumeration
{
    public static readonly ChannelStatus Inactive = new(0, nameof(Inactive));
    public static readonly ChannelStatus Active = new(1, nameof(Active));
    public static readonly ChannelStatus Error = new(2, nameof(Error));

    public ChannelStatus(int id, string name) : base(id, name) { }
}

/// <summary>
/// Channel health value object
/// </summary>
public class ChannelHealth : ValueObject
{
    public bool IsHealthy { get; }
    public string Status { get; }
    public string? ErrorMessage { get; }
    public DateTime LastChecked { get; }
    public Dictionary<string, object> Metrics { get; }

    private ChannelHealth(bool isHealthy, string status, string? errorMessage, DateTime lastChecked, Dictionary<string, object>? metrics)
    {
        IsHealthy = isHealthy;
        Status = status;
        ErrorMessage = errorMessage;
        LastChecked = lastChecked;
        Metrics = metrics ?? new Dictionary<string, object>();
    }

    // Parameterless constructor for EF Core
    private ChannelHealth()
    {
        IsHealthy = false;
        Status = "Unknown";
        ErrorMessage = null;
        LastChecked = DateTime.UtcNow;
        Metrics = new Dictionary<string, object>();
    }

    public static ChannelHealth Good(DateTime lastChecked, Dictionary<string, object>? metrics = null) =>
        new(true, "Good", null, lastChecked, metrics);

    public static ChannelHealth Bad(string errorMessage, DateTime lastChecked, Dictionary<string, object>? metrics = null) =>
        new(false, "Bad", errorMessage, lastChecked, metrics);

    public static ChannelHealth Unknown(Dictionary<string, object>? metrics = null) =>
        new(false, "Unknown", null, DateTime.UtcNow, metrics);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsHealthy;
        yield return Status;
        yield return ErrorMessage ?? string.Empty;
        yield return LastChecked;
        foreach (var metric in Metrics.OrderBy(x => x.Key))
        {
            yield return metric.Key;
            yield return metric.Value;
        }
    }
}

/// <summary>
/// Data point type enumeration
/// </summary>
public class DataPointType : Enumeration
{
    public static readonly DataPointType Boolean = new(0, nameof(Boolean));
    public static readonly DataPointType Byte = new(1, nameof(Byte));
    public static readonly DataPointType Int16 = new(2, nameof(Int16));
    public static readonly DataPointType Int32 = new(3, nameof(Int32));
    public static readonly DataPointType Int64 = new(4, nameof(Int64));
    public static readonly DataPointType Float = new(5, nameof(Float));
    public static readonly DataPointType Double = new(6, nameof(Double));
    public static readonly DataPointType String = new(7, nameof(String));
    public static readonly DataPointType DateTime = new(8, nameof(DateTime));

    public DataPointType(int id, string name) : base(id, name) { }
}

/// <summary>
/// Data point configuration value object
/// </summary>
public class DataPointConfiguration : ValueObject
{
    public TimeSpan ScanInterval { get; }
    public bool EnableLogging { get; }
    public double? ScalingFactor { get; }
    public double? Offset { get; }
    public object? DefaultValue { get; }
    public Dictionary<string, object> Properties { get; }

    public DataPointConfiguration(
        TimeSpan scanInterval = default,
        bool enableLogging = true,
        double? scalingFactor = null,
        double? offset = null,
        object? defaultValue = null,
        Dictionary<string, object>? properties = null)
    {
        ScanInterval = scanInterval == default ? TimeSpan.FromSeconds(1) : scanInterval;
        EnableLogging = enableLogging;
        ScalingFactor = scalingFactor;
        Offset = offset;
        DefaultValue = defaultValue;
        Properties = properties ?? new Dictionary<string, object>();
    }

    // Parameterless constructor for EF Core
    private DataPointConfiguration()
    {
        ScanInterval = TimeSpan.FromSeconds(1);
        EnableLogging = true;
        ScalingFactor = null;
        Offset = null;
        DefaultValue = null;
        Properties = new Dictionary<string, object>();
    }

    public static DataPointConfiguration Default() => new();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ScanInterval;
        yield return EnableLogging;
        yield return ScalingFactor ?? 0.0;
        yield return Offset ?? 0.0;
        yield return DefaultValue ?? string.Empty;
        foreach (var property in Properties.OrderBy(x => x.Key))
        {
            yield return property.Key;
            yield return property.Value;
        }
    }
}