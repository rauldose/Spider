using Spider.Core.SharedKernel.Base;

namespace Spider.DeviceManagement.Domain.Enums;

/// <summary>
/// Represents the connection status of a device
/// </summary>
public class DeviceStatus : Enumeration
{
    public static DeviceStatus Disconnected = new(1, nameof(Disconnected));
    public static DeviceStatus Connecting = new(2, nameof(Connecting));
    public static DeviceStatus Connected = new(3, nameof(Connected));
    public static DeviceStatus Error = new(4, nameof(Error));
    public static DeviceStatus Maintenance = new(5, nameof(Maintenance));

    public DeviceStatus(int id, string name) : base(id, name)
    {
    }
}