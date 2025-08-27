using Spider.Core.SharedKernel.Base;

namespace Spider.DeviceManagement.Domain.Enums;

/// <summary>
/// Represents the type of device protocol
/// </summary>
public class ProtocolType : Enumeration
{
    public static ProtocolType Modbus = new(1, nameof(Modbus));
    public static ProtocolType OpcUa = new(2, "OPC UA");
    public static ProtocolType Mqtt = new(3, "MQTT");
    public static ProtocolType TcpIp = new(4, "TCP/IP");
    public static ProtocolType SerialPort = new(5, "Serial Port");
    public static ProtocolType Ethernet = new(6, nameof(Ethernet));
    public static ProtocolType CanBus = new(7, "CAN Bus");
    public static ProtocolType Profinet = new(8, "PROFINET");
    public static ProtocolType EtherCat = new(9, "EtherCAT");
    public static ProtocolType Custom = new(10, nameof(Custom));

    public ProtocolType(int id, string name) : base(id, name)
    {
    }
}