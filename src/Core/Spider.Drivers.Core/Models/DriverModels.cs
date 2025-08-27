using Spider.Core.SharedKernel.Base;

namespace Spider.Drivers.Core.Models;

/// <summary>
/// Metadata information about a driver
/// </summary>
public class DriverMetadata : ValueObject
{
    public string Name { get; }
    public string Version { get; }
    public string Description { get; }
    public string Manufacturer { get; }
    public IReadOnlyList<string> SupportedProtocols { get; }
    public DateTime CreatedDate { get; }
    public string Author { get; }

    public DriverMetadata(
        string name,
        string version,
        string description,
        string manufacturer,
        IEnumerable<string> supportedProtocols,
        DateTime createdDate,
        string author)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
        SupportedProtocols = supportedProtocols?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(supportedProtocols));
        CreatedDate = createdDate;
        Author = author ?? throw new ArgumentNullException(nameof(author));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Version;
        yield return Description;
        yield return Manufacturer;
        yield return Author;
        yield return CreatedDate;
        foreach (var protocol in SupportedProtocols)
        {
            yield return protocol;
        }
    }
}

/// <summary>
/// Current status of a driver
/// </summary>
public class DriverStatus : Enumeration
{
    public static readonly DriverStatus Uninitialized = new(0, nameof(Uninitialized));
    public static readonly DriverStatus Initializing = new(1, nameof(Initializing));
    public static readonly DriverStatus Ready = new(2, nameof(Ready));
    public static readonly DriverStatus Connected = new(3, nameof(Connected));
    public static readonly DriverStatus Disconnected = new(4, nameof(Disconnected));
    public static readonly DriverStatus Error = new(5, nameof(Error));
    public static readonly DriverStatus Shutdown = new(6, nameof(Shutdown));

    public DriverStatus(int id, string name) : base(id, name) { }
}

/// <summary>
/// Capabilities supported by a driver
/// </summary>
public class DriverCapabilities : ValueObject
{
    public bool SupportsReading { get; }
    public bool SupportsWriting { get; }
    public bool SupportsSubscriptions { get; }
    public bool SupportsRealTime { get; }
    public bool SupportsDiagnostics { get; }
    public bool SupportsBulkOperations { get; }
    public int MaxConcurrentConnections { get; }
    public int MaxSubscriptions { get; }
    public TimeSpan MinPollingInterval { get; }
    public TimeSpan MaxPollingInterval { get; }
    public IReadOnlyList<string> SupportedDataTypes { get; }

    public DriverCapabilities(
        bool supportsReading = true,
        bool supportsWriting = false,
        bool supportsSubscriptions = false,
        bool supportsRealTime = false,
        bool supportsDiagnostics = true,
        bool supportsBulkOperations = false,
        int maxConcurrentConnections = 1,
        int maxSubscriptions = 0,
        TimeSpan minPollingInterval = default,
        TimeSpan maxPollingInterval = default,
        IEnumerable<string>? supportedDataTypes = null)
    {
        SupportsReading = supportsReading;
        SupportsWriting = supportsWriting;
        SupportsSubscriptions = supportsSubscriptions;
        SupportsRealTime = supportsRealTime;
        SupportsDiagnostics = supportsDiagnostics;
        SupportsBulkOperations = supportsBulkOperations;
        MaxConcurrentConnections = maxConcurrentConnections;
        MaxSubscriptions = maxSubscriptions;
        MinPollingInterval = minPollingInterval == default ? TimeSpan.FromMilliseconds(100) : minPollingInterval;
        MaxPollingInterval = maxPollingInterval == default ? TimeSpan.FromMinutes(10) : maxPollingInterval;
        SupportedDataTypes = supportedDataTypes?.ToList().AsReadOnly() ?? 
            new List<string> { "Boolean", "Byte", "Int16", "Int32", "Float", "String" }.AsReadOnly();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SupportsReading;
        yield return SupportsWriting;
        yield return SupportsSubscriptions;
        yield return SupportsRealTime;
        yield return SupportsDiagnostics;
        yield return SupportsBulkOperations;
        yield return MaxConcurrentConnections;
        yield return MaxSubscriptions;
        yield return MinPollingInterval;
        yield return MaxPollingInterval;
        foreach (var dataType in SupportedDataTypes)
        {
            yield return dataType;
        }
    }
}

/// <summary>
/// Configuration for driver initialization
/// </summary>
public class DriverConfiguration : ValueObject
{
    public string ConnectionString { get; }
    public Dictionary<string, object> Parameters { get; }
    public TimeSpan ConnectionTimeout { get; }
    public TimeSpan OperationTimeout { get; }
    public int MaxRetryAttempts { get; }
    public bool EnableDiagnostics { get; }

    public DriverConfiguration(
        string connectionString,
        Dictionary<string, object>? parameters = null,
        TimeSpan connectionTimeout = default,
        TimeSpan operationTimeout = default,
        int maxRetryAttempts = 3,
        bool enableDiagnostics = true)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        Parameters = parameters ?? new Dictionary<string, object>();
        ConnectionTimeout = connectionTimeout == default ? TimeSpan.FromSeconds(30) : connectionTimeout;
        OperationTimeout = operationTimeout == default ? TimeSpan.FromSeconds(10) : operationTimeout;
        MaxRetryAttempts = maxRetryAttempts;
        EnableDiagnostics = enableDiagnostics;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ConnectionString;
        yield return ConnectionTimeout;
        yield return OperationTimeout;
        yield return MaxRetryAttempts;
        yield return EnableDiagnostics;
        foreach (var parameter in Parameters.OrderBy(x => x.Key))
        {
            yield return parameter.Key;
            yield return parameter.Value;
        }
    }
}