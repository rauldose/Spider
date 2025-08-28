using Spider.Core.SharedKernel.Base;

namespace Spider.ConnectionManagement.Domain.ValueObjects;

public class ConnectionHealth : ValueObject
{
    public bool IsHealthy { get; }
    public double ResponseTimeMs { get; }
    public int ConsecutiveFailures { get; }
    public DateTime LastHealthCheck { get; }
    public string? LastError { get; }

    private ConnectionHealth(bool isHealthy, double responseTimeMs, int consecutiveFailures, DateTime lastHealthCheck, string? lastError)
    {
        IsHealthy = isHealthy;
        ResponseTimeMs = responseTimeMs;
        ConsecutiveFailures = consecutiveFailures;
        LastHealthCheck = lastHealthCheck;
        LastError = lastError;
    }

    public static ConnectionHealth Healthy(double responseTimeMs)
    {
        return new ConnectionHealth(true, responseTimeMs, 0, DateTime.UtcNow, null);
    }

    public static ConnectionHealth Unhealthy(string error, int consecutiveFailures = 1)
    {
        return new ConnectionHealth(false, 0, consecutiveFailures, DateTime.UtcNow, error);
    }

    public ConnectionHealth WithFailure(string error)
    {
        return new ConnectionHealth(false, 0, ConsecutiveFailures + 1, DateTime.UtcNow, error);
    }

    public ConnectionHealth WithSuccess(double responseTimeMs)
    {
        return new ConnectionHealth(true, responseTimeMs, 0, DateTime.UtcNow, null);
    }

    public bool IsCritical => ConsecutiveFailures >= 5;
    public bool RequiresAttention => ConsecutiveFailures >= 3;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsHealthy;
        yield return ResponseTimeMs;
        yield return ConsecutiveFailures;
        yield return LastHealthCheck;
        yield return LastError ?? string.Empty;
    }

    public override string ToString()
    {
        if (IsHealthy)
            return $"Healthy (Response: {ResponseTimeMs:F2}ms)";
        
        return $"Unhealthy (Failures: {ConsecutiveFailures}, Error: {LastError})";
    }
}