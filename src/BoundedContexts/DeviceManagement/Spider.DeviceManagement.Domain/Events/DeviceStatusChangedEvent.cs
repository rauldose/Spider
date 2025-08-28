using Spider.Core.SharedKernel.Events;

namespace Spider.DeviceManagement.Domain.Events;

/// <summary>
/// Domain event raised when a device status changes
/// </summary>
public class DeviceStatusChangedEvent : BaseDomainEvent
{
    public Guid DeviceId { get; }
    public string PreviousStatus { get; }
    public string NewStatus { get; }
    public string? Reason { get; }

    public DeviceStatusChangedEvent(Guid deviceId, string previousStatus, string newStatus, string? reason = null)
    {
        DeviceId = deviceId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason;
    }
}