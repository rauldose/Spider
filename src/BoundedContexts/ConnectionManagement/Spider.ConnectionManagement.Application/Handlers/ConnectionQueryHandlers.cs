using MediatR;
using Microsoft.Extensions.Logging;
using Spider.ConnectionManagement.Application.Queries;
using Spider.ConnectionManagement.Application.DTOs;
using Spider.ConnectionManagement.Application.Interfaces;
using Spider.ConnectionManagement.Domain.Entities;
using Spider.ConnectionManagement.Domain.Enumerations;

namespace Spider.ConnectionManagement.Application.Handlers;

public class GetConnectionByIdQueryHandler : IRequestHandler<GetConnectionByIdQuery, ConnectionDto?>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly ILogger<GetConnectionByIdQueryHandler> _logger;

    public GetConnectionByIdQueryHandler(
        IConnectionRepository connectionRepository,
        ILogger<GetConnectionByIdQueryHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<ConnectionDto?> Handle(GetConnectionByIdQuery request, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        return connection != null ? MapToDto(connection) : null;
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

public class GetConnectionsByDeviceIdQueryHandler : IRequestHandler<GetConnectionsByDeviceIdQuery, IEnumerable<ConnectionDto>>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly ILogger<GetConnectionsByDeviceIdQueryHandler> _logger;

    public GetConnectionsByDeviceIdQueryHandler(
        IConnectionRepository connectionRepository,
        ILogger<GetConnectionsByDeviceIdQueryHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ConnectionDto>> Handle(GetConnectionsByDeviceIdQuery request, CancellationToken cancellationToken)
    {
        var connections = await _connectionRepository.GetByDeviceIdAsync(request.DeviceId, cancellationToken);
        return connections.Select(MapToDto);
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

public class GetAllConnectionsQueryHandler : IRequestHandler<GetAllConnectionsQuery, IEnumerable<ConnectionDto>>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly ILogger<GetAllConnectionsQueryHandler> _logger;

    public GetAllConnectionsQueryHandler(
        IConnectionRepository connectionRepository,
        ILogger<GetAllConnectionsQueryHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ConnectionDto>> Handle(GetAllConnectionsQuery request, CancellationToken cancellationToken)
    {
        var connections = await _connectionRepository.GetAllAsync(cancellationToken);
        return connections.Select(MapToDto);
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

public class GetUnhealthyConnectionsQueryHandler : IRequestHandler<GetUnhealthyConnectionsQuery, IEnumerable<ConnectionDto>>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly ILogger<GetUnhealthyConnectionsQueryHandler> _logger;

    public GetUnhealthyConnectionsQueryHandler(
        IConnectionRepository connectionRepository,
        ILogger<GetUnhealthyConnectionsQueryHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ConnectionDto>> Handle(GetUnhealthyConnectionsQuery request, CancellationToken cancellationToken)
    {
        var connections = await _connectionRepository.GetUnhealthyAsync(cancellationToken);
        return connections.Select(MapToDto);
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

public class GetAvailableProtocolsQueryHandler : IRequestHandler<GetAvailableProtocolsQuery, IEnumerable<string>>
{
    private readonly IProtocolDriverFactory _protocolDriverFactory;
    private readonly ILogger<GetAvailableProtocolsQueryHandler> _logger;

    public GetAvailableProtocolsQueryHandler(
        IProtocolDriverFactory protocolDriverFactory,
        ILogger<GetAvailableProtocolsQueryHandler> logger)
    {
        _protocolDriverFactory = protocolDriverFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> Handle(GetAvailableProtocolsQuery request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(_protocolDriverFactory.GetSupportedProtocols());
    }
}

public class GetConnectionStatisticsQueryHandler : IRequestHandler<GetConnectionStatisticsQuery, ConnectionStatisticsDto>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly ILogger<GetConnectionStatisticsQueryHandler> _logger;

    public GetConnectionStatisticsQueryHandler(
        IConnectionRepository connectionRepository,
        ILogger<GetConnectionStatisticsQueryHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<ConnectionStatisticsDto> Handle(GetConnectionStatisticsQuery request, CancellationToken cancellationToken)
    {
        var allConnections = await _connectionRepository.GetAllAsync(cancellationToken);
        var connectionsList = allConnections.ToList();

        var totalConnections = connectionsList.Count;
        var connectedCount = connectionsList.Count(c => c.Status == ConnectionStatus.Connected);
        var disconnectedCount = connectionsList.Count(c => c.Status == ConnectionStatus.Disconnected);
        var failedCount = connectionsList.Count(c => c.Status == ConnectionStatus.Failed);
        var healthyCount = connectionsList.Count(c => c.Health.IsHealthy);
        var unhealthyCount = totalConnections - healthyCount;

        var averageResponseTime = connectionsList
            .Where(c => c.Health.IsHealthy && c.Health.ResponseTimeMs > 0)
            .Select(c => c.Health.ResponseTimeMs)
            .DefaultIfEmpty(0)
            .Average();

        return new ConnectionStatisticsDto(
            totalConnections,
            connectedCount,
            disconnectedCount,
            failedCount,
            healthyCount,
            unhealthyCount,
            averageResponseTime);
    }
}

public class GetConnectionHealthQueryHandler : IRequestHandler<GetConnectionHealthQuery, ConnectionHealthDto?>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly ILogger<GetConnectionHealthQueryHandler> _logger;

    public GetConnectionHealthQueryHandler(
        IConnectionRepository connectionRepository,
        ILogger<GetConnectionHealthQueryHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<ConnectionHealthDto?> Handle(GetConnectionHealthQuery request, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection == null) return null;

        return new ConnectionHealthDto(
            connection.Health.IsHealthy,
            connection.Health.ResponseTimeMs,
            connection.Health.ConsecutiveFailures,
            connection.Health.LastHealthCheck,
            connection.Health.LastError,
            connection.Health.IsCritical,
            connection.Health.RequiresAttention);
    }
}

public class GetConnectionStatusQueryHandler : IRequestHandler<GetConnectionStatusQuery, ConnectionStatusDto?>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly ILogger<GetConnectionStatusQueryHandler> _logger;

    public GetConnectionStatusQueryHandler(
        IConnectionRepository connectionRepository,
        ILogger<GetConnectionStatusQueryHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<ConnectionStatusDto?> Handle(GetConnectionStatusQuery request, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection == null) return null;

        return new ConnectionStatusDto(
            connection.Id,
            connection.Status.Name,
            connection.Health.IsHealthy,
            connection.Health.ResponseTimeMs,
            connection.Health.ConsecutiveFailures,
            connection.Health.LastHealthCheck,
            connection.Health.LastError);
    }
}