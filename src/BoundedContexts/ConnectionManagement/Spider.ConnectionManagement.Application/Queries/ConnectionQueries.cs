using Spider.Core.Application.Interfaces;
using Spider.ConnectionManagement.Application.DTOs;

namespace Spider.ConnectionManagement.Application.Queries;

public record GetConnectionByIdQuery(Guid ConnectionId) : IQuery<ConnectionDto?>;

public record GetConnectionsByDeviceIdQuery(Guid DeviceId) : IQuery<IEnumerable<ConnectionDto>>;

public record GetConnectionsByStatusQuery(string Status) : IQuery<IEnumerable<ConnectionDto>>;

public record GetAllConnectionsQuery() : IQuery<IEnumerable<ConnectionDto>>;

public record GetUnhealthyConnectionsQuery() : IQuery<IEnumerable<ConnectionDto>>;

public record GetConnectionHealthQuery(Guid ConnectionId) : IQuery<ConnectionHealthDto?>;

public record GetConnectionStatusQuery(Guid ConnectionId) : IQuery<ConnectionStatusDto?>;

public record GetAvailableProtocolsQuery() : IQuery<IEnumerable<string>>;

public record GetConnectionStatisticsQuery() : IQuery<ConnectionStatisticsDto>;

public record ConnectionStatisticsDto(
    int TotalConnections,
    int ConnectedCount,
    int DisconnectedCount,
    int FailedCount,
    int HealthyCount,
    int UnhealthyCount,
    double AverageResponseTime);