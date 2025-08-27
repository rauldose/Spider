using Spider.Core.SharedKernel.Base;
using Spider.DeviceManagement.Domain.Enums;
using Spider.DeviceManagement.Domain.Events;
using Spider.DeviceManagement.Domain.ValueObjects;

namespace Spider.DeviceManagement.Domain.Entities;

/// <summary>
/// Device aggregate root representing an IoT device in the system
/// </summary>
public class Device : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ProtocolType Protocol { get; private set; }
    public DeviceStatus Status { get; private set; }
    public ConnectionParameters ConnectionParameters { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset? LastConnectedAt { get; private set; }
    public Guid ProjectId { get; private set; }
    public bool IsEnabled { get; private set; }

    // For EF Core
    private Device() : base() 
    {
        Name = string.Empty;
        Description = string.Empty;
        Protocol = ProtocolType.Modbus;
        Status = DeviceStatus.Disconnected;
        ConnectionParameters = new ConnectionParameters("localhost", 502);
    }

    public Device(
        Guid id,
        string name,
        string description,
        ProtocolType protocol,
        ConnectionParameters connectionParameters,
        Guid projectId) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Device name cannot be null or empty.", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        Protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        ConnectionParameters = connectionParameters ?? throw new ArgumentNullException(nameof(connectionParameters));
        ProjectId = projectId;
        Status = DeviceStatus.Disconnected;
        IsEnabled = true;

        PublishDomainEvent(new DeviceCreatedEvent(Id, Name, Protocol.Name));
    }

    public void UpdateConnectionParameters(ConnectionParameters newParameters)
    {
        ConnectionParameters = newParameters ?? throw new ArgumentNullException(nameof(newParameters));
        MarkAsUpdated();
    }

    public void UpdateDescription(string description)
    {
        Description = description ?? string.Empty;
        MarkAsUpdated();
    }

    public void ChangeStatus(DeviceStatus newStatus, string? reason = null)
    {
        if (Status != newStatus)
        {
            var previousStatus = Status;
            Status = newStatus ?? throw new ArgumentNullException(nameof(newStatus));
            
            if (newStatus == DeviceStatus.Connected)
            {
                LastConnectedAt = DateTimeOffset.UtcNow;
                LastError = null;
            }
            else if (newStatus == DeviceStatus.Error && !string.IsNullOrEmpty(reason))
            {
                LastError = reason;
            }

            MarkAsUpdated();
            PublishDomainEvent(new DeviceStatusChangedEvent(Id, previousStatus.Name, newStatus.Name, reason));
        }
    }

    public void Enable()
    {
        IsEnabled = true;
        MarkAsUpdated();
    }

    public void Disable()
    {
        IsEnabled = false;
        if (Status == DeviceStatus.Connected || Status == DeviceStatus.Connecting)
        {
            ChangeStatus(DeviceStatus.Disconnected, "Device disabled");
        }
        MarkAsUpdated();
    }

    public bool CanConnect()
    {
        return IsEnabled && (Status == DeviceStatus.Disconnected || Status == DeviceStatus.Error);
    }
}