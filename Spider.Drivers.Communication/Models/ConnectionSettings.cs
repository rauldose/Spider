namespace Spider.Drivers.Communication.Models;

/// <summary>
/// Connection settings for PLC drivers
/// </summary>
public class ConnectionSettings
{
    /// <summary>
    /// IP address or hostname of the PLC
    /// </summary>
    public required string IpAddress { get; init; }

    /// <summary>
    /// Port number for the connection
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Connection timeout in milliseconds (default: 5000ms)
    /// </summary>
    public int ConnectTimeout { get; init; } = 5000;

    /// <summary>
    /// Receive timeout in milliseconds (default: 5000ms)
    /// </summary>
    public int ReceiveTimeout { get; init; } = 5000;

    /// <summary>
    /// Send timeout in milliseconds (default: 5000ms)
    /// </summary>
    public int SendTimeout { get; init; } = 5000;
}
