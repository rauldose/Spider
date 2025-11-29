namespace Spider.Drivers.Communication.Models;

/// <summary>
/// Connection settings for PLC drivers
/// </summary>
public class ConnectionSettings
{
    /// <summary>
    /// IP address or hostname of the PLC
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Port number for the connection
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Receive timeout in milliseconds
    /// </summary>
    public int ReceiveTimeout { get; set; } = 5000;

    /// <summary>
    /// Send timeout in milliseconds
    /// </summary>
    public int SendTimeout { get; set; } = 5000;
}
