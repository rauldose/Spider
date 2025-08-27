using MediatR;
using AutoMapper;
using Spider.Communication.Application.Queries;
using Spider.Communication.Application.DTOs;
using Spider.Communication.Application.Interfaces;
using Spider.Core.Application.Interfaces;
using Spider.Core.Application.Common;

namespace Spider.Communication.Application.Handlers;

/// <summary>
/// Link Query Handlers
/// </summary>
public class GetLinkByIdQueryHandler : IRequestHandler<GetLinkByIdQuery, IResult<LinkDto>>
{
    private readonly ILinkRepository _linkRepository;
    private readonly IMapper _mapper;

    public GetLinkByIdQueryHandler(ILinkRepository linkRepository, IMapper mapper)
    {
        _linkRepository = linkRepository;
        _mapper = mapper;
    }

    public async Task<IResult<LinkDto>> Handle(GetLinkByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var link = await _linkRepository.GetByIdWithChannelsAsync(request.LinkId, cancellationToken);
            if (link == null)
                return Result<LinkDto>.Failure("Link not found");

            var linkDto = _mapper.Map<LinkDto>(link);
            return Result<LinkDto>.Success(linkDto);
        }
        catch (Exception ex)
        {
            return Result<LinkDto>.Failure($"Failed to get link: {ex.Message}");
        }
    }
}

public class GetAllLinksQueryHandler : IRequestHandler<GetAllLinksQuery, IResult<IPagedResult<LinkDto>>>
{
    private readonly ILinkRepository _linkRepository;
    private readonly IMapper _mapper;

    public GetAllLinksQueryHandler(ILinkRepository linkRepository, IMapper mapper)
    {
        _linkRepository = linkRepository;
        _mapper = mapper;
    }

    public async Task<IResult<IPagedResult<LinkDto>>> Handle(GetAllLinksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pagedLinks = await _linkRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
            var linkDtos = _mapper.Map<List<LinkDto>>(pagedLinks.Items);

            var pagedResult = new PagedResult<LinkDto>(
                linkDtos,
                pagedLinks.TotalCount,
                request.Page,
                request.PageSize);

            return Result<IPagedResult<LinkDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<IPagedResult<LinkDto>>.Failure($"Failed to get links: {ex.Message}");
        }
    }
}

public class GetLinksByStatusQueryHandler : IRequestHandler<GetLinksByStatusQuery, IResult<IPagedResult<LinkDto>>>
{
    private readonly ILinkRepository _linkRepository;
    private readonly IMapper _mapper;

    public GetLinksByStatusQueryHandler(ILinkRepository linkRepository, IMapper mapper)
    {
        _linkRepository = linkRepository;
        _mapper = mapper;
    }

    public async Task<IResult<IPagedResult<LinkDto>>> Handle(GetLinksByStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pagedLinks = await _linkRepository.GetByStatusPagedAsync(request.Status, request.Page, request.PageSize, cancellationToken);
            var linkDtos = _mapper.Map<List<LinkDto>>(pagedLinks.Items);

            var pagedResult = new PagedResult<LinkDto>(
                linkDtos,
                pagedLinks.TotalCount,
                request.Page,
                request.PageSize);

            return Result<IPagedResult<LinkDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<IPagedResult<LinkDto>>.Failure($"Failed to get links by status: {ex.Message}");
        }
    }
}

/// <summary>
/// Channel Query Handlers
/// </summary>
public class GetChannelByIdQueryHandler : IRequestHandler<GetChannelByIdQuery, IResult<ChannelDto>>
{
    private readonly IChannelRepository _channelRepository;
    private readonly IMapper _mapper;

    public GetChannelByIdQueryHandler(IChannelRepository channelRepository, IMapper mapper)
    {
        _channelRepository = channelRepository;
        _mapper = mapper;
    }

    public async Task<IResult<ChannelDto>> Handle(GetChannelByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var channel = await _channelRepository.GetByIdWithDataPointsAsync(request.ChannelId, cancellationToken);
            if (channel == null)
                return Result<ChannelDto>.Failure("Channel not found");

            var channelDto = _mapper.Map<ChannelDto>(channel);
            return Result<ChannelDto>.Success(channelDto);
        }
        catch (Exception ex)
        {
            return Result<ChannelDto>.Failure($"Failed to get channel: {ex.Message}");
        }
    }
}

public class GetChannelsByLinkIdQueryHandler : IRequestHandler<GetChannelsByLinkIdQuery, IResult<List<ChannelDto>>>
{
    private readonly IChannelRepository _channelRepository;
    private readonly IMapper _mapper;

    public GetChannelsByLinkIdQueryHandler(IChannelRepository channelRepository, IMapper mapper)
    {
        _channelRepository = channelRepository;
        _mapper = mapper;
    }

    public async Task<IResult<List<ChannelDto>>> Handle(GetChannelsByLinkIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var channels = await _channelRepository.GetByLinkIdAsync(request.LinkId, cancellationToken);
            var channelDtos = _mapper.Map<List<ChannelDto>>(channels);
            return Result<List<ChannelDto>>.Success(channelDtos);
        }
        catch (Exception ex)
        {
            return Result<List<ChannelDto>>.Failure($"Failed to get channels: {ex.Message}");
        }
    }
}

/// <summary>
/// DataPoint Query Handlers
/// </summary>
public class GetDataPointByIdQueryHandler : IRequestHandler<GetDataPointByIdQuery, IResult<DataPointDto>>
{
    private readonly IDataPointRepository _dataPointRepository;
    private readonly IMapper _mapper;

    public GetDataPointByIdQueryHandler(IDataPointRepository dataPointRepository, IMapper mapper)
    {
        _dataPointRepository = dataPointRepository;
        _mapper = mapper;
    }

    public async Task<IResult<DataPointDto>> Handle(GetDataPointByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dataPoint = await _dataPointRepository.GetByIdAsync(request.DataPointId, cancellationToken);
            if (dataPoint == null)
                return Result<DataPointDto>.Failure("DataPoint not found");

            var dataPointDto = _mapper.Map<DataPointDto>(dataPoint);
            return Result<DataPointDto>.Success(dataPointDto);
        }
        catch (Exception ex)
        {
            return Result<DataPointDto>.Failure($"Failed to get data point: {ex.Message}");
        }
    }
}

public class GetDataPointsByChannelIdQueryHandler : IRequestHandler<GetDataPointsByChannelIdQuery, IResult<List<DataPointDto>>>
{
    private readonly IDataPointRepository _dataPointRepository;
    private readonly IMapper _mapper;

    public GetDataPointsByChannelIdQueryHandler(IDataPointRepository dataPointRepository, IMapper mapper)
    {
        _dataPointRepository = dataPointRepository;
        _mapper = mapper;
    }

    public async Task<IResult<List<DataPointDto>>> Handle(GetDataPointsByChannelIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dataPoints = await _dataPointRepository.GetByChannelIdAsync(request.ChannelId, cancellationToken);
            var dataPointDtos = _mapper.Map<List<DataPointDto>>(dataPoints);
            return Result<List<DataPointDto>>.Success(dataPointDtos);
        }
        catch (Exception ex)
        {
            return Result<List<DataPointDto>>.Failure($"Failed to get data points: {ex.Message}");
        }
    }
}

/// <summary>
/// Statistics Query Handlers
/// </summary>
public class GetCommunicationStatisticsQueryHandler : IRequestHandler<GetCommunicationStatisticsQuery, IResult<CommunicationStatisticsDto>>
{
    private readonly ILinkRepository _linkRepository;
    private readonly IChannelRepository _channelRepository;
    private readonly IDataPointRepository _dataPointRepository;

    public GetCommunicationStatisticsQueryHandler(
        ILinkRepository linkRepository,
        IChannelRepository channelRepository,
        IDataPointRepository dataPointRepository)
    {
        _linkRepository = linkRepository;
        _channelRepository = channelRepository;
        _dataPointRepository = dataPointRepository;
    }

    public async Task<IResult<CommunicationStatisticsDto>> Handle(GetCommunicationStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var totalLinks = await _linkRepository.CountAsync(cancellationToken);
            var connectedLinks = await _linkRepository.CountByStatusAsync("Connected", cancellationToken);
            var disconnectedLinks = await _linkRepository.CountByStatusAsync("Disconnected", cancellationToken);
            var healthyLinks = await _linkRepository.CountHealthyAsync(cancellationToken);

            var totalChannels = await _channelRepository.CountAsync(cancellationToken);
            var activeChannels = await _channelRepository.CountActiveAsync(cancellationToken);

            var totalDataPoints = await _dataPointRepository.CountAsync(cancellationToken);
            var activeDataPoints = await _dataPointRepository.CountActiveAsync(cancellationToken);

            // Calculate overall statistics
            var overallSuccessRate = totalLinks > 0 ? (double)healthyLinks / totalLinks * 100 : 0;
            var averageResponseTime = await _linkRepository.GetAverageResponseTimeAsync(cancellationToken);

            var statistics = new CommunicationStatisticsDto
            {
                TotalLinks = totalLinks,
                ConnectedLinks = connectedLinks,
                DisconnectedLinks = disconnectedLinks,
                HealthyLinks = healthyLinks,
                TotalChannels = totalChannels,
                ActiveChannels = activeChannels,
                TotalDataPoints = totalDataPoints,
                ActiveDataPoints = activeDataPoints,
                OverallSuccessRate = overallSuccessRate,
                AverageResponseTime = averageResponseTime
            };

            return Result<CommunicationStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            return Result<CommunicationStatisticsDto>.Failure($"Failed to get statistics: {ex.Message}");
        }
    }
}

public class GetLinkHealthQueryHandler : IRequestHandler<GetLinkHealthQuery, IResult<LinkHealthDto>>
{
    private readonly ILinkRepository _linkRepository;
    private readonly IMapper _mapper;

    public GetLinkHealthQueryHandler(ILinkRepository linkRepository, IMapper mapper)
    {
        _linkRepository = linkRepository;
        _mapper = mapper;
    }

    public async Task<IResult<LinkHealthDto>> Handle(GetLinkHealthQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var link = await _linkRepository.GetByIdAsync(request.LinkId, cancellationToken);
            if (link == null)
                return Result<LinkHealthDto>.Failure("Link not found");

            var healthDto = _mapper.Map<LinkHealthDto>(link.Health);
            return Result<LinkHealthDto>.Success(healthDto);
        }
        catch (Exception ex)
        {
            return Result<LinkHealthDto>.Failure($"Failed to get link health: {ex.Message}");
        }
    }
}