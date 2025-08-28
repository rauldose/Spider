using SpiderDriver.UnifiedAPI.Models;

namespace SpiderDriver.UnifiedAPI.Interfaces;

/// <summary>
/// Modern unified interface for all Spider drivers
/// Provides a consistent API across all industrial protocol drivers
/// </summary>
public interface IUnifiedDriver
{
    /// <summary>
    /// Driver unique identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Driver display name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Driver description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Protocol type (Modbus, OPC UA, MQTT, etc.)
    /// </summary>
    string ProtocolType { get; }

    /// <summary>
    /// Supported communication channels (TCP, UDP, Serial, etc.)
    /// </summary>
    IEnumerable<string> SupportedChannels { get; }

    /// <summary>
    /// Supported register types for this driver
    /// </summary>
    IEnumerable<string> SupportedRegisters { get; }

    /// <summary>
    /// Current connection status
    /// </summary>
    ConnectionStatus Status { get; }

    /// <summary>
    /// Driver capabilities
    /// </summary>
    DriverCapabilities Capabilities { get; }

    /// <summary>
    /// Connect to the device/service
    /// </summary>
    Task<bool> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from the device/service
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Read data from device
    /// </summary>
    Task<ReadResult> ReadAsync(ReadRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write data to device
    /// </summary>
    Task<WriteResult> WriteAsync(WriteRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe to real-time data updates
    /// </summary>
    Task<IDataSubscription> SubscribeAsync(SubscriptionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get driver configuration schema
    /// </summary>
    DriverConfigurationSchema GetConfigurationSchema();

    /// <summary>
    /// Validate tag/variable configuration
    /// </summary>
    ValidationResult ValidateTagConfiguration(TagConfiguration tag);

    /// <summary>
    /// Event for connection status changes
    /// </summary>
    event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

    /// <summary>
    /// Event for data value changes
    /// </summary>
    event EventHandler<DataValueChangedEventArgs> DataValueChanged;

    /// <summary>
    /// Event for driver errors
    /// </summary>
    event EventHandler<DriverErrorEventArgs> DriverError;
}