using MediatR;
using Microsoft.Extensions.Logging;
using Spider.Core.SharedKernel.Abstractions;
using Spider.ConnectionManagement.Application.Commands;
using Spider.ConnectionManagement.Application.DTOs;
using Spider.ConnectionManagement.Application.Interfaces;
using Spider.ConnectionManagement.Domain.Entities;
using Spider.ConnectionManagement.Domain.ValueObjects;
using Spider.ConnectionManagement.Domain.Exceptions;

namespace Spider.ConnectionManagement.Application.Handlers;

public class UpdateConnectionParametersCommandHandler : IRequestHandler<UpdateConnectionParametersCommand, ConnectionDto>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateConnectionParametersCommandHandler> _logger;

    public UpdateConnectionParametersCommandHandler(
        IConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateConnectionParametersCommandHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ConnectionDto> Handle(UpdateConnectionParametersCommand request, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken)
            ?? throw new ConnectionNotFoundException(request.ConnectionId);

        var newParameters = ConnectionParameters.Create(
            request.Host,
            request.Port,
            request.TimeoutMs,
            request.RetryAttempts,
            request.ExtendedProperties);

        connection.UpdateParameters(newParameters);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated parameters for connection {ConnectionId}", request.ConnectionId);

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

public class DeleteConnectionCommandHandler : IRequestHandler<DeleteConnectionCommand, bool>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnectionMonitorService _monitorService;
    private readonly ILogger<DeleteConnectionCommandHandler> _logger;

    public DeleteConnectionCommandHandler(
        IConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork,
        IConnectionMonitorService monitorService,
        ILogger<DeleteConnectionCommandHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
        _monitorService = monitorService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteConnectionCommand request, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection == null) return false;

        // Stop monitoring if active
        await _monitorService.StopMonitoringAsync(connection.Id, cancellationToken);

        // Disconnect if connected
        if (connection.CanDisconnect())
        {
            connection.Disconnect("Connection being deleted");
        }

        await _connectionRepository.RemoveAsync(connection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted connection {ConnectionId}", request.ConnectionId);
        return true;
    }
}

public class TestConnectionCommandHandler : IRequestHandler<TestConnectionCommand, bool>
{
    private readonly IProtocolDriverFactory _protocolDriverFactory;
    private readonly ILogger<TestConnectionCommandHandler> _logger;

    public TestConnectionCommandHandler(
        IProtocolDriverFactory protocolDriverFactory,
        ILogger<TestConnectionCommandHandler> logger)
    {
        _protocolDriverFactory = protocolDriverFactory;
        _logger = logger;
    }

    public async Task<bool> Handle(TestConnectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var driver = _protocolDriverFactory.CreateDriver(request.Protocol);
            var parameters = ConnectionParameters.Create(
                request.Host,
                request.Port,
                request.TimeoutMs,
                0, // No retries for test
                request.ExtendedProperties);

            var result = await driver.TestConnectionAsync(parameters, cancellationToken);
            
            _logger.LogInformation("Connection test for {Protocol} to {Host}:{Port} - Result: {Result}", 
                request.Protocol, request.Host, request.Port, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed for {Protocol} to {Host}:{Port}", 
                request.Protocol, request.Host, request.Port);
            return false;
        }
    }
}