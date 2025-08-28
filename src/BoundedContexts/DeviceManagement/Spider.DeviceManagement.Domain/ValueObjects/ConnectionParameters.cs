using Spider.Core.SharedKernel.Base;

namespace Spider.DeviceManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing connection parameters for a device
/// </summary>
public class ConnectionParameters : ValueObject
{
    public string Host { get; }
    public int Port { get; }
    public int? Timeout { get; }
    public int? RetryCount { get; }
    public Dictionary<string, string> AdditionalParameters { get; }

    public ConnectionParameters(
        string host, 
        int port, 
        int? timeout = null, 
        int? retryCount = null, 
        Dictionary<string, string>? additionalParameters = null)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty.", nameof(host));
        
        if (port <= 0 || port > 65535)
            throw new ArgumentException("Port must be between 1 and 65535.", nameof(port));
        
        if (timeout.HasValue && timeout.Value <= 0)
            throw new ArgumentException("Timeout must be positive.", nameof(timeout));
        
        if (retryCount.HasValue && retryCount.Value < 0)
            throw new ArgumentException("Retry count cannot be negative.", nameof(retryCount));

        Host = host;
        Port = port;
        Timeout = timeout;
        RetryCount = retryCount;
        AdditionalParameters = additionalParameters ?? new Dictionary<string, string>();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Host;
        yield return Port;
        yield return Timeout;
        yield return RetryCount;
        
        foreach (var param in AdditionalParameters.OrderBy(x => x.Key))
        {
            yield return param.Key;
            yield return param.Value;
        }
    }

    public string GetConnectionString()
    {
        var baseConnection = $"{Host}:{Port}";
        
        if (Timeout.HasValue)
            baseConnection += $";Timeout={Timeout.Value}";
            
        if (RetryCount.HasValue)
            baseConnection += $";RetryCount={RetryCount.Value}";
            
        foreach (var param in AdditionalParameters)
        {
            baseConnection += $";{param.Key}={param.Value}";
        }
        
        return baseConnection;
    }
}