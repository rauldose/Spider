using MediatR;
using Microsoft.Extensions.Logging;
using Spider.Core.SharedKernel.Abstractions;
using Spider.ConnectionManagement.Application.Commands;
using Spider.ConnectionManagement.Application.DTOs;
using Spider.ConnectionManagement.Application.Interfaces;
using Spider.ConnectionManagement.Domain.Entities;
using Spider.ConnectionManagement.Domain.Enumerations;
using Spider.ConnectionManagement.Domain.ValueObjects;
using Spider.ConnectionManagement.Domain.Exceptions;

namespace Spider.ConnectionManagement.Application.Handlers;

public class CreateConnectionCommandHandler : IRequestHandler<CreateConnectionCommand, ConnectionDto>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProtocolDriverFactory _protocolDriverFactory;
    private readonly ILogger<CreateConnectionCommandHandler> _logger;

    public CreateConnectionCommandHandler(
        IConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork,
        IProtocolDriverFactory protocolDriverFactory,
        ILogger<CreateConnectionCommandHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
        _protocolDriverFactory = protocolDriverFactory;
        _logger = logger;
    }

    public async Task<ConnectionDto> Handle(CreateConnectionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating connection for device {DeviceId} with protocol {Protocol}", 
            request.DeviceId, request.Protocol);

        // Check if connection already exists for this device and name
        var exists = await _connectionRepository.ExistsByDeviceIdAndNameAsync(
            request.DeviceId, request.Name, cancellationToken);
        
        if (exists)
        {
            throw new ConnectionException($"Connection with name '{request.Name}' already exists for device {request.DeviceId}");
        }

        // Validate protocol
        var supportedProtocols = _protocolDriverFactory.GetSupportedProtocols();
        if (!supportedProtocols.Contains(request.Protocol, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidConnectionParametersException($"Unsupported protocol: {request.Protocol}");
        }

        var protocolType = ProtocolType.GetAll<ProtocolType>()
            .FirstOrDefault(p => p.Name.Equals(request.Protocol, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidConnectionParametersException($"Invalid protocol: {request.Protocol}");

        var parameters = ConnectionParameters.Create(
            request.Host,
            request.Port,
            request.TimeoutMs,
            request.RetryAttempts,
            request.ExtendedProperties);

        var connection = Connection.Create(request.DeviceId, request.Name, protocolType, parameters);

        await _connectionRepository.AddAsync(connection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created connection {ConnectionId} for device {DeviceId}", 
            connection.Id, request.DeviceId);

        return MapToDto(connection);
    }

    private static ConnectionDto MapToDto(Connection connection)
    {
        return new ConnectionDto(
            connection.Id,
            connection.DeviceId,
            connection.Name,
            connection.Protocol.Name,
            connection.Parameters.Host,
            connection.Parameters.Port,
            connection.Status.Name,
            connection.Health.IsHealthy,
            connection.Health.ResponseTimeMs,
            connection.Health.ConsecutiveFailures,
            connection.CreatedAt,
            connection.LastConnectedAt,
            connection.LastDisconnectedAt,
            connection.LastErrorMessage,
            connection.GetUptime());
    }
}

public class ConnectCommandHandler : IRequestHandler<ConnectCommand, bool>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProtocolDriverFactory _protocolDriverFactory;
    private readonly IConnectionMonitorService _monitorService;
    private readonly ILogger<ConnectCommandHandler> _logger;

    public ConnectCommandHandler(
        IConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork,
        IProtocolDriverFactory protocolDriverFactory,
        IConnectionMonitorService monitorService,
        ILogger<ConnectCommandHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
        _protocolDriverFactory = protocolDriverFactory;
        _monitorService = monitorService;
        _logger = logger;
    }

    public async Task<bool> Handle(ConnectCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to connect connection {ConnectionId}", request.ConnectionId);

        var connection = await _connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken)
            ?? throw new ConnectionNotFoundException(request.ConnectionId);

        if (!connection.CanConnect())
        {
            _logger.LogWarning("Connection {ConnectionId} cannot be connected in current status: {Status}", 
                request.ConnectionId, connection.Status.Name);
            return false;
        }

        try
        {
            connection.Connect();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Attempt actual connection
            var driver = _protocolDriverFactory.CreateDriver(connection.Protocol.Name);
            var physicalConnection = await driver.ConnectAsync(connection.Parameters, cancellationToken);

            if (physicalConnection.IsConnected)
            {
                connection.MarkConnected();
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Start monitoring
                await _monitorService.StartMonitoringAsync(connection.Id, cancellationToken);

                _logger.LogInformation("Successfully connected connection {ConnectionId}", request.ConnectionId);
                return true;
            }
            else
            {
                connection.MarkFailed("Failed to establish physical connection");
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect connection {ConnectionId}", request.ConnectionId);
            connection.MarkFailed(ex.Message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return false;
        }
    }
}

public class DisconnectCommandHandler : IRequestHandler<DisconnectCommand, bool>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnectionMonitorService _monitorService;
    private readonly ILogger<DisconnectCommandHandler> _logger;

    public DisconnectCommandHandler(
        IConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork,
        IConnectionMonitorService monitorService,
        ILogger<DisconnectCommandHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
        _monitorService = monitorService;
        _logger = logger;
    }

    public async Task<bool> Handle(DisconnectCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disconnecting connection {ConnectionId}", request.ConnectionId);

        var connection = await _connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken)
            ?? throw new ConnectionNotFoundException(request.ConnectionId);

        if (!connection.CanDisconnect())
        {
            _logger.LogWarning("Connection {ConnectionId} cannot be disconnected in current status: {Status}", 
                request.ConnectionId, connection.Status.Name);
            return false;
        }

        try
        {
            // Stop monitoring first
            await _monitorService.StopMonitoringAsync(connection.Id, cancellationToken);

            connection.Disconnect(request.Reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully disconnected connection {ConnectionId}", request.ConnectionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect connection {ConnectionId}", request.ConnectionId);
            return false;
        }
    }
}