namespace Spider.Drivers.Communication.Models;

/// <summary>
/// Connection settings for Modbus TCP driver
/// </summary>
public sealed class ModbusTcpSettings : ConnectionSettings
{
    /// <summary>
    /// Slave/Unit ID (default: 1)
    /// </summary>
    public byte SlaveId { get; init; } = 1;

    /// <summary>
    /// Maximum number of registers per read request (default: 125)
    /// </summary>
    public ushort MaxReadRegisters { get; init; } = 125;

    /// <summary>
    /// Maximum number of coils per read request (default: 2000)
    /// </summary>
    public ushort MaxReadCoils { get; init; } = 2000;
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
public sealed class SiemensS7Settings : ConnectionSettings
{
    /// <summary>
    /// Siemens PLC type (default: S1200)
    /// </summary>
    public SiemensPlcType PlcType { get; init; } = SiemensPlcType.S1200;

    /// <summary>
    /// Rack number (default: 0)
    /// </summary>
    public byte Rack { get; init; } = 0;

    /// <summary>
    /// Slot number (default: 0 for S1200/S1500, 2 for S300/S400)
    /// </summary>
    public byte Slot { get; init; } = 0;

    /// <summary>
    /// Connection type (1 = PG, 2 = OP, 3 = Basic)
    /// </summary>
    public byte ConnectionType { get; init; } = 3;

    /// <summary>
    /// Local TSAP (default: 0x0100)
    /// </summary>
    public int LocalTSAP { get; init; } = 0x0100;
}

/// <summary>
/// Connection settings for AllenBradley CIP driver
/// </summary>
public sealed class AllenBradleyCipSettings : ConnectionSettings
{
    /// <summary>
    /// Slot number of the processor (default: 0)
    /// </summary>
    public byte Slot { get; init; } = 0;

    /// <summary>
    /// Optional message router path
    /// </summary>
    public string? RouterPath { get; init; }
}
