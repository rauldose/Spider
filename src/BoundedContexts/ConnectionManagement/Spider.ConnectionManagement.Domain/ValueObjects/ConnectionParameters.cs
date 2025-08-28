using Spider.Core.SharedKernel.Base;
using System.Text.Json;

namespace Spider.ConnectionManagement.Domain.ValueObjects;

public class ConnectionParameters : ValueObject
{
    public string Host { get; }
    public int Port { get; }
    public int TimeoutMs { get; }
    public int RetryAttempts { get; }
    public Dictionary<string, object> ExtendedProperties { get; }

    private ConnectionParameters(string host, int port, int timeoutMs, int retryAttempts, Dictionary<string, object> extendedProperties)
    {
        Host = host;
        Port = port;
        TimeoutMs = timeoutMs;
        RetryAttempts = retryAttempts;
        ExtendedProperties = extendedProperties;
    }

    public static ConnectionParameters Create(string host, int port, int timeoutMs = 5000, int retryAttempts = 3, Dictionary<string, object>? extendedProperties = null)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty", nameof(host));
        
        if (port <= 0 || port > 65535)
            throw new ArgumentException("Port must be between 1 and 65535", nameof(port));
        
        if (timeoutMs <= 0)
            throw new ArgumentException("Timeout must be greater than 0", nameof(timeoutMs));
        
        if (retryAttempts < 0)
            throw new ArgumentException("Retry attempts cannot be negative", nameof(retryAttempts));

        return new ConnectionParameters(host, port, timeoutMs, retryAttempts, extendedProperties ?? new Dictionary<string, object>());
    }

    public ConnectionParameters WithTimeout(int timeoutMs)
    {
        return Create(Host, Port, timeoutMs, RetryAttempts, ExtendedProperties);
    }

    public ConnectionParameters WithRetryAttempts(int retryAttempts)
    {
        return Create(Host, Port, TimeoutMs, retryAttempts, ExtendedProperties);
    }

    public ConnectionParameters WithExtendedProperty(string key, object value)
    {
        var newProperties = new Dictionary<string, object>(ExtendedProperties)
        {
            [key] = value
        };
        return Create(Host, Port, TimeoutMs, RetryAttempts, newProperties);
    }

    public T? GetExtendedProperty<T>(string key)
    {
        if (ExtendedProperties.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }
            if (value is T directValue)
            {
                return directValue;
            }
        }
        return default;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Host;
        yield return Port;
        yield return TimeoutMs;
        yield return RetryAttempts;
        
        foreach (var kvp in ExtendedProperties.OrderBy(x => x.Key))
        {
            yield return kvp.Key;
            yield return kvp.Value;
        }
    }

    public override string ToString()
    {
        var extended = ExtendedProperties.Any() 
            ? $" ({string.Join(", ", ExtendedProperties.Select(kvp => $"{kvp.Key}={kvp.Value}"))})"
            : "";
        return $"{Host}:{Port} (Timeout: {TimeoutMs}ms, Retries: {RetryAttempts}){extended}";
    }
}