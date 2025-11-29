namespace Spider.Drivers.Communication.Models;

/// <summary>
/// Connection settings for Modbus TCP driver
/// </summary>
public class ModbusTcpSettings : ConnectionSettings
{
    public ModbusTcpSettings()
    {
        Port = 502; // Default Modbus TCP port
    }

    /// <summary>
    /// Slave/Unit ID (default: 1)
    /// </summary>
    public byte SlaveId { get; set; } = 1;

    /// <summary>
    /// Maximum number of registers per read request
    /// </summary>
    public ushort MaxReadRegisters { get; set; } = 125;

    /// <summary>
    /// Maximum number of coils per read request
    /// </summary>
    public ushort MaxReadCoils { get; set; } = 2000;
}

/// <summary>
/// Siemens PLC types
/// </summary>
public enum SiemensPlcType
{
    S1200 = 0,
    S300 = 1,
    S400 = 2,
    S1500 = 3,
    S200Smart = 4,
    S200 = 5
}

/// <summary>
/// Connection settings for Siemens S7 driver
/// </summary>
public class SiemensS7Settings : ConnectionSettings
{
    public SiemensS7Settings()
    {
        Port = 102; // Default S7 port
    }

    /// <summary>
    /// Siemens PLC type
    /// </summary>
    public SiemensPlcType PlcType { get; set; } = SiemensPlcType.S1200;

    /// <summary>
    /// Rack number (default: 0)
    /// </summary>
    public byte Rack { get; set; } = 0;

    /// <summary>
    /// Slot number (default: 0 for S1200/S1500, 2 for S300/S400)
    /// </summary>
    public byte Slot { get; set; } = 0;

    /// <summary>
    /// Connection type (1 = PG, 2 = OP, 3 = Basic)
    /// </summary>
    public byte ConnectionType { get; set; } = 3;

    /// <summary>
    /// Local TSAP
    /// </summary>
    public int LocalTSAP { get; set; } = 0x0100;
}

/// <summary>
/// Connection settings for AllenBradley CIP driver
/// </summary>
public class AllenBradleyCipSettings : ConnectionSettings
{
    public AllenBradleyCipSettings()
    {
        Port = 44818; // Default EtherNet/IP port
    }

    /// <summary>
    /// Slot number of the processor (default: 0)
    /// </summary>
    public byte Slot { get; set; } = 0;

    /// <summary>
    /// Optional message router path
    /// </summary>
    public string? RouterPath { get; set; }
}
