using MediatR;
using Spider.Communication.Application.DTOs;
using Spider.Core.Application.Interfaces;
using Spider.Core.Application.Common;

namespace Spider.Communication.Application.Queries;

/// <summary>
/// Link Queries
/// </summary>
public record GetLinkByIdQuery(Guid LinkId) : IRequest<IResult<LinkDto>>;

public record GetAllLinksQuery(int Page = 1, int PageSize = 10) : IRequest<IResult<IPagedResult<LinkDto>>>;

public record GetLinksByStatusQuery(string Status, int Page = 1, int PageSize = 10) : IRequest<IResult<IPagedResult<LinkDto>>>;

public record GetLinksByProtocolQuery(string ProtocolType, int Page = 1, int PageSize = 10) : IRequest<IResult<IPagedResult<LinkDto>>>;

public record SearchLinksQuery(string SearchTerm, int Page = 1, int PageSize = 10) : IRequest<IResult<IPagedResult<LinkDto>>>;

/// <summary>
/// Channel Queries
/// </summary>
public record GetChannelByIdQuery(Guid ChannelId) : IRequest<IResult<ChannelDto>>;

public record GetChannelsByLinkIdQuery(Guid LinkId) : IRequest<IResult<List<ChannelDto>>>;

public record GetAllChannelsQuery(int Page = 1, int PageSize = 10) : IRequest<IResult<IPagedResult<ChannelDto>>>;

public record GetActiveChannelsQuery() : IRequest<IResult<List<ChannelDto>>>;

/// <summary>
/// DataPoint Queries
/// </summary>
public record GetDataPointByIdQuery(Guid DataPointId) : IRequest<IResult<DataPointDto>>;

public record GetDataPointsByChannelIdQuery(Guid ChannelId) : IRequest<IResult<List<DataPointDto>>>;

public record GetDataPointsByLinkIdQuery(Guid LinkId) : IRequest<IResult<List<DataPointDto>>>;

public record GetAllDataPointsQuery(int Page = 1, int PageSize = 10) : IRequest<IResult<IPagedResult<DataPointDto>>>;

public record GetActiveDataPointsQuery() : IRequest<IResult<List<DataPointDto>>>;

public record GetDataPointsByAddressQuery(string Address) : IRequest<IResult<List<DataPointDto>>>;

/// <summary>
/// Statistics and Monitoring Queries
/// </summary>
public record GetCommunicationStatisticsQuery() : IRequest<IResult<CommunicationStatisticsDto>>;

public record GetLinkHealthQuery(Guid LinkId) : IRequest<IResult<LinkHealthDto>>;

public record GetAllLinksHealthQuery() : IRequest<IResult<List<LinkHealthDto>>>;

/// <summary>
/// Real-time and Data Queries
/// </summary>
public record GetRealTimeDataQuery(Guid LinkId) : IRequest<IResult<Dictionary<string, object>>>;

public record GetDataPointValueQuery(Guid DataPointId) : IRequest<IResult<object>>;

public record GetDataPointHistoryQuery(Guid DataPointId, DateTime StartTime, DateTime EndTime) : IRequest<IResult<List<DataValueDto>>>;

/// <summary>
/// Supporting DTOs for queries
/// </summary>
public record DataValueDto
{
    public DateTime Timestamp { get; init; }
    public object Value { get; init; } = new();
    public string Quality { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
}