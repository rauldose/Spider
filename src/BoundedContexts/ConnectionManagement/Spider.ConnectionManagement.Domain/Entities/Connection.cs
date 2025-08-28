using Spider.Core.SharedKernel.Base;
using Spider.ConnectionManagement.Domain.Enumerations;
using Spider.ConnectionManagement.Domain.ValueObjects;
using Spider.ConnectionManagement.Domain.Events;
using Spider.ConnectionManagement.Domain.Exceptions;

namespace Spider.ConnectionManagement.Domain.Entities;

public class Connection : AggregateRoot<Guid>
{
    public Guid DeviceId { get; private set; }
    public string Name { get; private set; }
    public ProtocolType Protocol { get; private set; }
    public ConnectionParameters Parameters { get; private set; }
    public ConnectionStatus Status { get; private set; }
    public ConnectionHealth Health { get; private set; }
    public new DateTime CreatedAt { get; private set; }
    public DateTime? LastConnectedAt { get; private set; }
    public DateTime? LastDisconnectedAt { get; private set; }
    public string? LastErrorMessage { get; private set; }

    // For EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private Connection() { }
#pragma warning restore CS8618

    private Connection(Guid id, Guid deviceId, string name, ProtocolType protocol, ConnectionParameters parameters)
        : base(id)
    {
        DeviceId = deviceId;
        Name = name;
        Protocol = protocol;
        Parameters = parameters;
        Status = ConnectionStatus.Disconnected;
        Health = ConnectionHealth.Unhealthy("Not connected");
        CreatedAt = DateTime.UtcNow;
    }

    public static Connection Create(Guid deviceId, string name, ProtocolType protocol, ConnectionParameters parameters)
    {
        if (deviceId == Guid.Empty)
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Connection name cannot be null or empty", nameof(name));

        var connection = new Connection(Guid.NewGuid(), deviceId, name, protocol, parameters);
        
        connection.PublishDomainEvent(new ConnectionCreatedEvent(
            connection.Id,
            connection.DeviceId,
            connection.Name,
            connection.Protocol.Name,
            connection.Parameters.Host,
            connection.Parameters.Port));

        return connection;
    }

    public void Connect()
    {
        if (Status == ConnectionStatus.Connected)
            return;

        var previousStatus = Status;
        Status = ConnectionStatus.Connecting;
        LastErrorMessage = null;

        PublishDomainEvent(new ConnectionStatusChangedEvent(
            Id,
            DeviceId,
            previousStatus.Name,
            Status.Name,
            null));
    }

    public void MarkConnected()
    {
        if (Status == ConnectionStatus.Connected)
            return;

        var previousStatus = Status;
        Status = ConnectionStatus.Connected;
        LastConnectedAt = DateTime.UtcNow;
        LastErrorMessage = null;
        Health = ConnectionHealth.Healthy(0);

        PublishDomainEvent(new ConnectionStatusChangedEvent(
            Id,
            DeviceId,
            previousStatus.Name,
            Status.Name,
            null));

        PublishDomainEvent(new ConnectionEstablishedEvent(
            Id,
            DeviceId,
            Name,
            Protocol.Name));
    }

    public void Disconnect(string? reason = null)
    {
        if (Status == ConnectionStatus.Disconnected)
            return;

        var previousStatus = Status;
        Status = ConnectionStatus.Disconnected;
        LastDisconnectedAt = DateTime.UtcNow;
        LastErrorMessage = reason;
        Health = ConnectionHealth.Unhealthy(reason ?? "Manually disconnected");

        PublishDomainEvent(new ConnectionStatusChangedEvent(
            Id,
            DeviceId,
            previousStatus.Name,
            Status.Name,
            reason));

        PublishDomainEvent(new ConnectionClosedEvent(
            Id,
            DeviceId,
            Name,
            reason));
    }

    public void MarkFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));

        var previousStatus = Status;
        Status = ConnectionStatus.Failed;
        LastErrorMessage = errorMessage;
        Health = Health.WithFailure(errorMessage);

        PublishDomainEvent(new ConnectionStatusChangedEvent(
            Id,
            DeviceId,
            previousStatus.Name,
            Status.Name,
            errorMessage));

        PublishDomainEvent(new ConnectionFailedEvent(
            Id,
            DeviceId,
            Name,
            errorMessage,
            Health.ConsecutiveFailures));
    }

    public void StartReconnection()
    {
        if (Status == ConnectionStatus.Connected || Status == ConnectionStatus.Connecting)
            return;

        var previousStatus = Status;
        Status = ConnectionStatus.Reconnecting;

        PublishDomainEvent(new ConnectionStatusChangedEvent(
            Id,
            DeviceId,
            previousStatus.Name,
            Status.Name,
            "Attempting reconnection"));
    }

    public void UpdateParameters(ConnectionParameters newParameters)
    {
        if (Status == ConnectionStatus.Connected)
            throw new ConnectionException("Cannot update parameters while connected. Disconnect first.");

        var oldParameters = Parameters;
        Parameters = newParameters;

        PublishDomainEvent(new ConnectionParametersUpdatedEvent(
            Id,
            DeviceId,
            oldParameters.ToString(),
            newParameters.ToString()));
    }

    public void UpdateHealth(ConnectionHealth newHealth)
    {
        Health = newHealth;

        PublishDomainEvent(new ConnectionHealthUpdatedEvent(
            Id,
            DeviceId,
            Health.IsHealthy,
            Health.ResponseTimeMs,
            Health.ConsecutiveFailures,
            Health.LastError));
    }

    public bool CanConnect()
    {
        return Status == ConnectionStatus.Disconnected || Status == ConnectionStatus.Failed;
    }

    public bool CanDisconnect()
    {
        return Status == ConnectionStatus.Connected || Status == ConnectionStatus.Connecting || Status == ConnectionStatus.Reconnecting;
    }

    public TimeSpan? GetUptime()
    {
        if (Status != ConnectionStatus.Connected || LastConnectedAt == null)
            return null;

        return DateTime.UtcNow - LastConnectedAt.Value;
    }
}