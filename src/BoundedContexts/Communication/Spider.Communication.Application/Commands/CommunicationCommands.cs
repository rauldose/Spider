using MediatR;
using Spider.Communication.Application.DTOs;
using Spider.Core.Application.Interfaces;
using Spider.Core.Application.Common;

namespace Spider.Communication.Application.Commands;

/// <summary>
/// Link Commands
/// </summary>
public record CreateLinkCommand(CreateLinkDto LinkDto) : IRequest<IResult<LinkDto>>;

public record UpdateLinkCommand(UpdateLinkDto LinkDto) : IRequest<IResult<LinkDto>>;

public record DeleteLinkCommand(Guid LinkId) : IRequest<IResult<bool>>;

public record ConnectLinkCommand(Guid LinkId) : IRequest<IResult<bool>>;

public record DisconnectLinkCommand(Guid LinkId) : IRequest<IResult<bool>>;

public record AttachDriverToLinkCommand(Guid LinkId, string DriverType, Dictionary<string, object> DriverConfiguration) : IRequest<IResult<bool>>;

/// <summary>
/// Channel Commands
/// </summary>
public record CreateChannelCommand(CreateChannelDto ChannelDto) : IRequest<IResult<ChannelDto>>;

public record UpdateChannelCommand(Guid ChannelId, string Name, string Description, string ChannelType) : IRequest<IResult<ChannelDto>>;

public record DeleteChannelCommand(Guid ChannelId) : IRequest<IResult<bool>>;

public record EnableChannelCommand(Guid ChannelId) : IRequest<IResult<bool>>;

public record DisableChannelCommand(Guid ChannelId) : IRequest<IResult<bool>>;

/// <summary>
/// DataPoint Commands
/// </summary>
public record CreateDataPointCommand(CreateDataPointDto DataPointDto) : IRequest<IResult<DataPointDto>>;

public record UpdateDataPointCommand(
    Guid DataPointId, 
    string Name, 
    string Description, 
    string Address, 
    string DataType, 
    string AccessMode) : IRequest<IResult<DataPointDto>>;

public record DeleteDataPointCommand(Guid DataPointId) : IRequest<IResult<bool>>;

public record EnableDataPointCommand(Guid DataPointId) : IRequest<IResult<bool>>;

public record DisableDataPointCommand(Guid DataPointId) : IRequest<IResult<bool>>;

public record ReadDataPointCommand(Guid DataPointId) : IRequest<IResult<object>>;

public record WriteDataPointCommand(Guid DataPointId, object Value) : IRequest<IResult<bool>>;

public record BulkReadDataPointsCommand(List<Guid> DataPointIds) : IRequest<IResult<Dictionary<Guid, object>>>;

/// <summary>
/// Operational Commands
/// </summary>
public record StartRealTimeDataCollectionCommand(Guid LinkId) : IRequest<IResult<bool>>;

public record StopRealTimeDataCollectionCommand(Guid LinkId) : IRequest<IResult<bool>>;

public record DiagnoseLinkCommand(Guid LinkId) : IRequest<IResult<Dictionary<string, object>>>;

public record HealthCheckLinkCommand(Guid LinkId) : IRequest<IResult<LinkHealthDto>>;