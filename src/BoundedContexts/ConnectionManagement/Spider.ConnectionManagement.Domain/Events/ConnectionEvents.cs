using Spider.Core.SharedKernel.Events;

namespace Spider.ConnectionManagement.Domain.Events;

public class ConnectionCreatedEvent : BaseDomainEvent
{
    public Guid ConnectionId { get; }
    public Guid DeviceId { get; }
    public string Name { get; }
    public string Protocol { get; }
    public string Host { get; }
    public int Port { get; }

    public ConnectionCreatedEvent(Guid connectionId, Guid deviceId, string name, string protocol, string host, int port)
    {
        ConnectionId = connectionId;
        DeviceId = deviceId;
        Name = name;
        Protocol = protocol;
        Host = host;
        Port = port;
    }
}

public class ConnectionStatusChangedEvent : BaseDomainEvent
{
    public Guid ConnectionId { get; }
    public Guid DeviceId { get; }
    public string PreviousStatus { get; }
    public string NewStatus { get; }
    public string? Reason { get; }

    public ConnectionStatusChangedEvent(Guid connectionId, Guid deviceId, string previousStatus, string newStatus, string? reason)
    {
        ConnectionId = connectionId;
        DeviceId = deviceId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason;
    }
}

public class ConnectionEstablishedEvent : BaseDomainEvent
{
    public Guid ConnectionId { get; }
    public Guid DeviceId { get; }
    public string Name { get; }
    public string Protocol { get; }

    public ConnectionEstablishedEvent(Guid connectionId, Guid deviceId, string name, string protocol)
    {
        ConnectionId = connectionId;
        DeviceId = deviceId;
        Name = name;
        Protocol = protocol;
    }
}

public class ConnectionClosedEvent : BaseDomainEvent
{
    public Guid ConnectionId { get; }
    public Guid DeviceId { get; }
    public string Name { get; }
    public string? Reason { get; }

    public ConnectionClosedEvent(Guid connectionId, Guid deviceId, string name, string? reason)
    {
        ConnectionId = connectionId;
        DeviceId = deviceId;
        Name = name;
        Reason = reason;
    }
}

public class ConnectionFailedEvent : BaseDomainEvent
{
    public Guid ConnectionId { get; }
    public Guid DeviceId { get; }
    public string Name { get; }
    public string ErrorMessage { get; }
    public int ConsecutiveFailures { get; }

    public ConnectionFailedEvent(Guid connectionId, Guid deviceId, string name, string errorMessage, int consecutiveFailures)
    {
        ConnectionId = connectionId;
        DeviceId = deviceId;
        Name = name;
        ErrorMessage = errorMessage;
        ConsecutiveFailures = consecutiveFailures;
    }
}

public class ConnectionParametersUpdatedEvent : BaseDomainEvent
{
    public Guid ConnectionId { get; }
    public Guid DeviceId { get; }
    public string OldParameters { get; }
    public string NewParameters { get; }

    public ConnectionParametersUpdatedEvent(Guid connectionId, Guid deviceId, string oldParameters, string newParameters)
    {
        ConnectionId = connectionId;
        DeviceId = deviceId;
        OldParameters = oldParameters;
        NewParameters = newParameters;
    }
}

public class ConnectionHealthUpdatedEvent : BaseDomainEvent
{
    public Guid ConnectionId { get; }
    public Guid DeviceId { get; }
    public bool IsHealthy { get; }
    public double ResponseTimeMs { get; }
    public int ConsecutiveFailures { get; }
    public string? LastError { get; }

    public ConnectionHealthUpdatedEvent(Guid connectionId, Guid deviceId, bool isHealthy, double responseTimeMs, int consecutiveFailures, string? lastError)
    {
        ConnectionId = connectionId;
        DeviceId = deviceId;
        IsHealthy = isHealthy;
        ResponseTimeMs = responseTimeMs;
        ConsecutiveFailures = consecutiveFailures;
        LastError = lastError;
    }
}