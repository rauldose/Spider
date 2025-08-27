namespace SpiderDriver.UnifiedAPI.Models;

/// <summary>
/// Connection parameters for driver configuration
/// </summary>
public class ConnectionParameters
{
    public string HostOrPath { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, object> AdditionalParameters { get; set; } = new();
}

/// <summary>
/// Request for reading data from device
/// </summary>
public class ReadRequest
{
    public string[] Addresses { get; set; } = Array.Empty<string>();
    public string RegisterType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Result of a read operation
/// </summary>
public class ReadResult
{
    public bool IsSuccess { get; set; }
    public Dictionary<string, object> Values { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request for writing data to device
/// </summary>
public class WriteRequest
{
    public Dictionary<string, object> Values { get; set; } = new();
    public string RegisterType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Result of a write operation
/// </summary>
public class WriteResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request for subscribing to data changes
/// </summary>
public class SubscriptionRequest
{
    public string[] Addresses { get; set; } = Array.Empty<string>();
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(1);
    public string RegisterType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Data subscription interface
/// </summary>
public interface IDataSubscription : IDisposable
{
    string Id { get; }
    bool IsActive { get; }
    Task UnsubscribeAsync();
    event EventHandler<DataValueChangedEventArgs> DataChanged;
}

/// <summary>
/// Tag configuration for validation
/// </summary>
public class TagConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string RegisterType { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Driver configuration schema
/// </summary>
public class DriverConfigurationSchema
{
    public string DriverType { get; set; } = string.Empty;
    public List<ConfigurationProperty> Properties { get; set; } = new();
    public List<RegisterTypeDefinition> RegisterTypes { get; set; } = new();
}

/// <summary>
/// Configuration property definition
/// </summary>
public class ConfigurationProperty
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Type PropertyType { get; set; } = typeof(string);
    public bool IsRequired { get; set; }
    public object? DefaultValue { get; set; }
    public List<object> AllowedValues { get; set; } = new();
}

/// <summary>
/// Register type definition
/// </summary>
public class RegisterTypeDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AddressFormat { get; set; } = string.Empty;
    public List<string> SupportedDataTypes { get; set; } = new();
}