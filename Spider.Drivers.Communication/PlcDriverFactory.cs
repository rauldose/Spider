using Spider.Drivers.Communication.Drivers.AllenBradley;
using Spider.Drivers.Communication.Drivers.Modbus;
using Spider.Drivers.Communication.Drivers.Siemens;
using Spider.Drivers.Communication.Interfaces;
using Spider.Drivers.Communication.Models;

namespace Spider.Drivers.Communication;

/// <summary>
/// Factory for creating PLC driver instances
/// </summary>
public static class PlcDriverFactory
{
    /// <summary>
    /// Create a Modbus TCP driver
    /// </summary>
    /// <param name="settings">Modbus TCP settings</param>
    /// <returns>A new ModbusTcpDriver instance</returns>
    public static IPlcDriver CreateModbusTcp(ModbusTcpSettings settings)
    {
        return new ModbusTcpDriver(settings);
    }

    /// <summary>
    /// Create a Modbus TCP driver with basic settings
    /// </summary>
    /// <param name="ipAddress">IP address of the Modbus device</param>
    /// <param name="port">Port number (default: 502)</param>
    /// <param name="slaveId">Slave ID (default: 1)</param>
    /// <returns>A new ModbusTcpDriver instance</returns>
    public static IPlcDriver CreateModbusTcp(string ipAddress, int port = 502, byte slaveId = 1)
    {
        return new ModbusTcpDriver(new ModbusTcpSettings
        {
            IpAddress = ipAddress,
            Port = port,
            SlaveId = slaveId
        });
    }

    /// <summary>
    /// Create a Siemens S7 driver
    /// </summary>
    /// <param name="settings">Siemens S7 settings</param>
    /// <returns>A new SiemensS7Driver instance</returns>
    public static IPlcDriver CreateSiemensS7(SiemensS7Settings settings)
    {
        return new SiemensS7Driver(settings);
    }

    /// <summary>
    /// Create a Siemens S7 driver with basic settings
    /// </summary>
    /// <param name="ipAddress">IP address of the PLC</param>
    /// <param name="plcType">Type of Siemens PLC</param>
    /// <param name="rack">Rack number (default: 0)</param>
    /// <param name="slot">Slot number (default: 0 for S1200/S1500, 2 for S300/S400)</param>
    /// <returns>A new SiemensS7Driver instance</returns>
    public static IPlcDriver CreateSiemensS7(string ipAddress, SiemensPlcType plcType = SiemensPlcType.S1200, byte rack = 0, byte slot = 0)
    {
        return new SiemensS7Driver(new SiemensS7Settings
        {
            IpAddress = ipAddress,
            PlcType = plcType,
            Rack = rack,
            Slot = slot
        });
    }

    /// <summary>
    /// Create an AllenBradley CIP driver
    /// </summary>
    /// <param name="settings">AllenBradley CIP settings</param>
    /// <returns>A new AllenBradleyCipDriver instance</returns>
    public static IPlcDriver CreateAllenBradleyCip(AllenBradleyCipSettings settings)
    {
        return new AllenBradleyCipDriver(settings);
    }

    /// <summary>
    /// Create an AllenBradley CIP driver with basic settings
    /// </summary>
    /// <param name="ipAddress">IP address of the PLC</param>
    /// <param name="slot">Processor slot (default: 0)</param>
    /// <returns>A new AllenBradleyCipDriver instance</returns>
    public static IPlcDriver CreateAllenBradleyCip(string ipAddress, byte slot = 0)
    {
        return new AllenBradleyCipDriver(new AllenBradleyCipSettings
        {
            IpAddress = ipAddress,
            Slot = slot
        });
    }
}
