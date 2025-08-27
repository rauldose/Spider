using Spider.Core.SharedKernel.Base;

namespace Spider.ConnectionManagement.Domain.Enumerations;

public class ProtocolType : Enumeration
{
    public static readonly ProtocolType Modbus = new(1, nameof(Modbus), "Modbus TCP/RTU protocol for industrial devices");
    public static readonly ProtocolType OpcUa = new(2, nameof(OpcUa), "OPC UA protocol for modern industrial automation");
    public static readonly ProtocolType Mqtt = new(3, nameof(Mqtt), "MQTT protocol for IoT messaging");
    public static readonly ProtocolType EthernetIp = new(4, nameof(EthernetIp), "Ethernet/IP protocol for Allen-Bradley PLCs");
    public static readonly ProtocolType Siemens = new(5, nameof(Siemens), "Siemens S7 protocol for Siemens PLCs");
    public static readonly ProtocolType Omron = new(6, nameof(Omron), "Omron FINS protocol for Omron PLCs");
    public static readonly ProtocolType Mitsubishi = new(7, nameof(Mitsubishi), "Mitsubishi MC protocol for Mitsubishi PLCs");

    public string Description { get; }

    private ProtocolType(int id, string name, string description) : base(id, name)
    {
        Description = description;
    }

    public bool SupportsPolling => this != Mqtt; // MQTT is event-driven
    public bool RequiresAuthentication => this == OpcUa || this == Mqtt;
}