namespace Spider.ConnectionManagement.Application.DTOs;

public record ConnectionDto(
    Guid Id,
    Guid DeviceId,
    string Name,
    string Protocol,
    string Host,
    int Port,
    string Status,
    bool IsHealthy,
    double ResponseTimeMs,
    int ConsecutiveFailures,
    DateTime CreatedAt,
    DateTime? LastConnectedAt,
    DateTime? LastDisconnectedAt,
    string? LastErrorMessage,
    TimeSpan? Uptime);

public record CreateConnectionDto(
    Guid DeviceId,
    string Name,
    string Protocol,
    string Host,
    int Port,
    int TimeoutMs = 5000,
    int RetryAttempts = 3,
    Dictionary<string, object>? ExtendedProperties = null);

public record UpdateConnectionParametersDto(
    string Host,
    int Port,
    int TimeoutMs,
    int RetryAttempts,
    Dictionary<string, object>? ExtendedProperties = null);

public record ConnectionStatusDto(
    Guid Id,
    string Status,
    bool IsHealthy,
    double ResponseTimeMs,
    int ConsecutiveFailures,
    DateTime LastHealthCheck,
    string? LastError);

public record ConnectionHealthDto(
    bool IsHealthy,
    double ResponseTimeMs,
    int ConsecutiveFailures,
    DateTime LastHealthCheck,
    string? LastError,
    bool IsCritical,
    bool RequiresAttention);