using Microsoft.Extensions.Logging;
using Spider.ConnectionManagement.Application.Interfaces;
using Spider.ConnectionManagement.Domain.Entities;
using Spider.ConnectionManagement.Domain.ValueObjects;
using Spider.ConnectionManagement.Domain.Enumerations;
using Spider.Drivers.Core.Abstractions;
using Spider.Drivers.Core.Models;

namespace Spider.ConnectionManagement.Infrastructure.Services;

/// <summary>
/// Enhanced connection service that integrates with the new driver architecture
/// This demonstrates how to refactor existing bounded contexts to use the new driver abstractions
/// </summary>
public class EnhancedConnectionService : IConnectionService
{
    private readonly IDriverManager _driverManager;
    private readonly IDriverFactory _driverFactory;
    private readonly IConnectionRepository _connectionRepository;
    private readonly ILogger<EnhancedConnectionService> _logger;
    private readonly Dictionary<Guid, string> _connectionDriverMap = new();

    public EnhancedConnectionService(
        IDriverManager driverManager,
        IDriverFactory driverFactory,
        IConnectionRepository connectionRepository,
        ILogger<EnhancedConnectionService> logger)
    {
        _driverManager = driverManager ?? throw new ArgumentNullException(nameof(driverManager));
        _driverFactory = driverFactory ?? throw new ArgumentNullException(nameof(driverFactory));
        _connectionRepository = connectionRepository ?? throw new ArgumentNullException(nameof(connectionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new connection using the enhanced driver architecture
    /// </summary>
    public async Task<Connection> CreateConnectionAsync(string name, Guid deviceId, ConnectionParameters parameters, string protocolType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating enhanced connection {ConnectionName} for device {DeviceId}", name, deviceId);

            // Get the protocol type enumeration
            var protocol = ProtocolType.FromDisplayName<ProtocolType>(protocolType);
            
            // Create the connection entity
            var connection = Connection.Create(deviceId, name, protocol, parameters);

            // Create driver configuration from connection parameters
            var driverConfig = new DriverConfiguration(
                connectionString: $"{parameters.Host}:{parameters.Port}",
                parameters: parameters.ExtendedProperties,
                connectionTimeout: TimeSpan.FromMilliseconds(parameters.TimeoutMs),
                operationTimeout: TimeSpan.FromMilliseconds(parameters.TimeoutMs / 2),
                maxRetryAttempts: parameters.RetryAttempts,
                enableDiagnostics: true);

            // Create and register the driver
            var driverId = await _driverManager.CreateDriverAsync(protocolType, driverConfig, cancellationToken);
            _connectionDriverMap[connection.Id] = driverId;

            // Save the connection
            await _connectionRepository.AddAsync(connection, cancellationToken);

            _logger.LogInformation("Enhanced connection {ConnectionId} created successfully with driver {DriverId}", connection.Id, driverId);
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create enhanced connection {ConnectionName}", name);
            throw;
        }
    }

    /// <summary>
    /// Connect using the enhanced driver architecture
    /// </summary>
    public async Task<bool> ConnectAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionRepository.GetByIdAsync(connectionId, cancellationToken);
            if (connection == null)
            {
                _logger.LogWarning("Connection {ConnectionId} not found", connectionId);
                return false;
            }

            if (!_connectionDriverMap.TryGetValue(connectionId, out var driverId))
            {
                _logger.LogWarning("No driver found for connection {ConnectionId}", connectionId);
                return false;
            }

            var driver = await _driverManager.GetDriverAsync(driverId, cancellationToken);
            if (driver == null)
            {
                _logger.LogWarning("Driver {DriverId} not found for connection {ConnectionId}", driverId, connectionId);
                return false;
            }

            _logger.LogInformation("Connecting {ConnectionId} using enhanced driver {DriverId}", connectionId, driverId);

            // Update connection status
            connection.Connect();

            // Driver should already be initialized by the manager
            // Just verify it's ready for use
            if (driver.Status != DriverStatus.Ready && driver.Status != DriverStatus.Connected)
            {
                _logger.LogWarning("Driver {DriverId} is not ready (Status: {Status})", driverId, driver.Status.Name);
                connection.Disconnect("Driver not ready");
                return false;
            }

            await _connectionRepository.UpdateAsync(connection, cancellationToken);

            _logger.LogInformation("Connection {ConnectionId} established successfully", connectionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect {ConnectionId}", connectionId);
            return false;
        }
    }

    /// <summary>
    /// Disconnect using the enhanced driver architecture
    /// </summary>
    public async Task<bool> DisconnectAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionRepository.GetByIdAsync(connectionId, cancellationToken);
            if (connection == null)
            {
                _logger.LogWarning("Connection {ConnectionId} not found", connectionId);
                return false;
            }

            _logger.LogInformation("Disconnecting {ConnectionId}", connectionId);

            // Update connection status
            connection.Disconnect();
            await _connectionRepository.UpdateAsync(connection, cancellationToken);

            // Note: We don't shutdown the driver here as it might be shared
            // The driver manager handles driver lifecycle

            _logger.LogInformation("Connection {ConnectionId} disconnected successfully", connectionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect {ConnectionId}", connectionId);
            return false;
        }
    }

    /// <summary>
    /// Test connection using the enhanced driver architecture
    /// </summary>
    public async Task<bool> TestConnectionAsync(ConnectionParameters parameters, string protocolType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing connection to {Host}:{Port} using protocol {Protocol}", 
                parameters.Host, parameters.Port, protocolType);

            // Create a temporary driver for testing
            var driver = _driverFactory.CreateDriver(protocolType);

            using (driver)
            {
                var driverConfig = new DriverConfiguration(
                    connectionString: $"{parameters.Host}:{parameters.Port}",
                    parameters: parameters.ExtendedProperties,
                    connectionTimeout: TimeSpan.FromMilliseconds(parameters.TimeoutMs),
                    operationTimeout: TimeSpan.FromMilliseconds(parameters.TimeoutMs / 2));

                // Initialize and test the driver
                var initResult = await driver.InitializeAsync(driverConfig, cancellationToken);
                if (!initResult.Success)
                {
                    _logger.LogWarning("Driver initialization failed during test: {Error}", initResult.ErrorMessage);
                    return false;
                }

                // Perform health check
                var healthResult = await driver.HealthCheckAsync(cancellationToken);
                var success = healthResult.IsHealthy;

                _logger.LogInformation("Connection test result for {Host}:{Port}: {Result}", 
                    parameters.Host, parameters.Port, success ? "Success" : "Failed");

                return success;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed for {Host}:{Port}", parameters.Host, parameters.Port);
            return false;
        }
    }

    /// <summary>
    /// Get enhanced connection health using driver diagnostics
    /// </summary>
    public async Task<ConnectionHealth> GetConnectionHealthAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connectionDriverMap.TryGetValue(connectionId, out var driverId))
            {
                return ConnectionHealth.Unhealthy("No driver associated with connection");
            }

            var driver = await _driverManager.GetDriverAsync(driverId, cancellationToken);
            if (driver == null)
            {
                return ConnectionHealth.Unhealthy("Driver not found");
            }

            // Get comprehensive health information from the driver
            var healthResult = await driver.HealthCheckAsync(cancellationToken);
            
            if (healthResult.IsHealthy)
            {
                // Get additional diagnostics if available
                if (driver is IDiagnosticDriver diagnosticDriver)
                {
                    var diagnostics = await diagnosticDriver.GetDiagnosticsAsync(cancellationToken);
                    var metrics = await diagnosticDriver.GetPerformanceMetricsAsync(cancellationToken);
                    
                    return ConnectionHealth.Healthy(metrics.AverageLatency);
                }
                else
                {
                    return ConnectionHealth.Healthy(0);
                }
            }
            else
            {
                return ConnectionHealth.Unhealthy(healthResult.ErrorMessage ?? "Driver health check failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connection health for {ConnectionId}", connectionId);
            return ConnectionHealth.Unhealthy($"Health check error: {ex.Message}");
        }
    }

    /// <summary>
    /// Read data using the enhanced driver architecture
    /// </summary>
    public async Task<object?> ReadDataAsync(Guid connectionId, string address, string dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connectionDriverMap.TryGetValue(connectionId, out var driverId))
            {
                throw new InvalidOperationException("No driver associated with connection");
            }

            var driver = await _driverManager.GetDriverAsync(driverId, cancellationToken);
            if (driver == null)
            {
                throw new InvalidOperationException("Driver not found");
            }

            if (driver is not IReadableDriver readableDriver)
            {
                throw new InvalidOperationException("Driver does not support reading operations");
            }

            var request = new ReadRequest(address, dataType);
            var result = await readableDriver.ReadAsync(request, cancellationToken);

            if (result.Success)
            {
                _logger.LogDebug("Successfully read data from {Address}: {Value}", address, result.Value);
                return result.Value;
            }
            else
            {
                _logger.LogWarning("Failed to read data from {Address}: {Error}", address, result.ErrorMessage);
                throw new InvalidOperationException($"Read operation failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read data from {Address} on connection {ConnectionId}", address, connectionId);
            throw;
        }
    }

    /// <summary>
    /// Write data using the enhanced driver architecture
    /// </summary>
    public async Task<bool> WriteDataAsync(Guid connectionId, string address, object value, string dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connectionDriverMap.TryGetValue(connectionId, out var driverId))
            {
                throw new InvalidOperationException("No driver associated with connection");
            }

            var driver = await _driverManager.GetDriverAsync(driverId, cancellationToken);
            if (driver == null)
            {
                throw new InvalidOperationException("Driver not found");
            }

            if (driver is not IWritableDriver writableDriver)
            {
                throw new InvalidOperationException("Driver does not support writing operations");
            }

            var request = new WriteRequest(address, value, dataType);
            var result = await writableDriver.WriteAsync(request, cancellationToken);

            if (result.Success)
            {
                _logger.LogDebug("Successfully wrote data to {Address}: {Value}", address, value);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to write data to {Address}: {Error}", address, result.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write data to {Address} on connection {ConnectionId}", address, connectionId);
            return false;
        }
    }

    /// <summary>
    /// Get driver statistics for monitoring
    /// </summary>
    public async Task<Dictionary<string, object>> GetDriverStatisticsAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connectionDriverMap.TryGetValue(connectionId, out var driverId))
            {
                return new Dictionary<string, object> { { "Error", "No driver associated with connection" } };
            }

            var driver = await _driverManager.GetDriverAsync(driverId, cancellationToken);
            if (driver == null)
            {
                return new Dictionary<string, object> { { "Error", "Driver not found" } };
            }

            var statistics = new Dictionary<string, object>
            {
                { "DriverId", driverId },
                { "DriverName", driver.Metadata.Name },
                { "DriverVersion", driver.Metadata.Version },
                { "Status", driver.Status.Name },
                { "SupportedProtocols", driver.Metadata.SupportedProtocols },
                { "Capabilities", new
                    {
                        SupportsReading = driver.Capabilities.SupportsReading,
                        SupportsWriting = driver.Capabilities.SupportsWriting,
                        SupportsSubscriptions = driver.Capabilities.SupportsSubscriptions,
                        SupportsRealTime = driver.Capabilities.SupportsRealTime,
                        SupportsDiagnostics = driver.Capabilities.SupportsDiagnostics,
                        MaxConnections = driver.Capabilities.MaxConcurrentConnections
                    }
                }
            };

            // Add performance metrics if available
            if (driver is IDiagnosticDriver diagnosticDriver)
            {
                var metrics = await diagnosticDriver.GetPerformanceMetricsAsync(cancellationToken);
                statistics["PerformanceMetrics"] = new
                {
                    TotalOperations = metrics.TotalOperations,
                    SuccessfulOperations = metrics.SuccessfulOperations,
                    FailedOperations = metrics.FailedOperations,
                    SuccessRate = metrics.SuccessRate,
                    AverageLatency = metrics.AverageLatency,
                    MinLatency = metrics.MinLatency,
                    MaxLatency = metrics.MaxLatency
                };
            }

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get driver statistics for connection {ConnectionId}", connectionId);
            return new Dictionary<string, object> { { "Error", ex.Message } };
        }
    }
}

/// <summary>
/// Interface for enhanced connection service - extends existing interface
/// </summary>
public interface IConnectionService
{
    Task<Connection> CreateConnectionAsync(string name, Guid deviceId, ConnectionParameters parameters, string protocolType, CancellationToken cancellationToken = default);
    Task<bool> ConnectAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(ConnectionParameters parameters, string protocolType, CancellationToken cancellationToken = default);
    Task<ConnectionHealth> GetConnectionHealthAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<object?> ReadDataAsync(Guid connectionId, string address, string dataType, CancellationToken cancellationToken = default);
    Task<bool> WriteDataAsync(Guid connectionId, string address, object value, string dataType, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetDriverStatisticsAsync(Guid connectionId, CancellationToken cancellationToken = default);
}