using Spider.Core.SharedKernel.Events;

namespace Spider.DeviceManagement.Domain.Events;

/// <summary>
/// Domain event raised when a device is created
/// </summary>
public class DeviceCreatedEvent : BaseDomainEvent
{
    public Guid DeviceId { get; }
    public string DeviceName { get; }
    public string ProtocolType { get; }

    public DeviceCreatedEvent(Guid deviceId, string deviceName, string protocolType)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        ProtocolType = protocolType;
    }
}