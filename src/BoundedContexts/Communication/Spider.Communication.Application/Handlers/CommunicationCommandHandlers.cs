using MediatR;
using AutoMapper;
using Spider.Communication.Application.Commands;
using Spider.Communication.Application.DTOs;
using Spider.Communication.Application.Interfaces;
using Spider.Communication.Domain.Entities;
using Spider.Communication.Domain.ValueObjects;
using Spider.Core.Application.Interfaces;
using Spider.Core.Application.Common;
using Spider.Core.SharedKernel.Abstractions;
using Spider.Drivers.Core.Abstractions;

namespace Spider.Communication.Application.Handlers;

/// <summary>
/// Link Command Handlers
/// </summary>
public class CreateLinkCommandHandler : IRequestHandler<CreateLinkCommand, IResult<LinkDto>>
{
    private readonly ILinkRepository _linkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDriverFactory _driverFactory;

    public CreateLinkCommandHandler(
        ILinkRepository linkRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDriverFactory driverFactory)
    {
        _linkRepository = linkRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _driverFactory = driverFactory;
    }

    public async Task<IResult<LinkDto>> Handle(CreateLinkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create link metadata and configuration
            var metadata = new LinkMetadata(
                request.LinkDto.Name,
                request.LinkDto.Description,
                request.LinkDto.ProtocolType);

            var configuration = new LinkConfiguration(
                request.LinkDto.Configuration.ConnectionString,
                request.LinkDto.Configuration.Parameters,
                request.LinkDto.Configuration.ConnectionTimeout,
                request.LinkDto.Configuration.ReadTimeout, // operationTimeout
                request.LinkDto.Configuration.HeartbeatInterval, // healthCheckInterval
                10, // maxChannels - default
                request.LinkDto.Configuration.EnableHeartbeat, // autoReconnect
                request.LinkDto.Configuration.MaxRetries); // maxRetryAttempts

            // Create link entity
            var link = new Link(metadata, configuration);

            // Add to repository
            await _linkRepository.AddAsync(link, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO
            var linkDto = _mapper.Map<LinkDto>(link);
            return Result<LinkDto>.Success(linkDto);
        }
        catch (Exception ex)
        {
            return Result<LinkDto>.Failure($"Failed to create link: {ex.Message}");
        }
    }
}

public class UpdateLinkCommandHandler : IRequestHandler<UpdateLinkCommand, IResult<LinkDto>>
{
    private readonly ILinkRepository _linkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateLinkCommandHandler(
        ILinkRepository linkRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _linkRepository = linkRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IResult<LinkDto>> Handle(UpdateLinkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var link = await _linkRepository.GetByIdAsync(request.LinkDto.Id, cancellationToken);
            if (link == null)
                return Result<LinkDto>.Failure("Link not found");

            // Update link properties
            var newConfiguration = new LinkConfiguration(
                request.LinkDto.Configuration.ConnectionString,
                request.LinkDto.Configuration.Parameters,
                request.LinkDto.Configuration.ConnectionTimeout,
                request.LinkDto.Configuration.ReadTimeout, // operationTimeout
                request.LinkDto.Configuration.HeartbeatInterval, // healthCheckInterval
                10, // maxChannels - default
                request.LinkDto.Configuration.EnableHeartbeat, // autoReconnect
                request.LinkDto.Configuration.MaxRetries); // maxRetryAttempts

            link.UpdateConfiguration(newConfiguration);

            await _linkRepository.UpdateAsync(link, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var linkDto = _mapper.Map<LinkDto>(link);
            return Result<LinkDto>.Success(linkDto);
        }
        catch (Exception ex)
        {
            return Result<LinkDto>.Failure($"Failed to update link: {ex.Message}");
        }
    }
}

public class ConnectLinkCommandHandler : IRequestHandler<ConnectLinkCommand, IResult<bool>>
{
    private readonly ILinkRepository _linkRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConnectLinkCommandHandler(
        ILinkRepository linkRepository,
        IUnitOfWork unitOfWork)
    {
        _linkRepository = linkRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult<bool>> Handle(ConnectLinkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var link = await _linkRepository.GetByIdAsync(request.LinkId, cancellationToken);
            if (link == null)
                return Result<bool>.Failure("Link not found");

            var result = await link.ConnectAsync(cancellationToken);
            if (result.Success)
            {
                await _linkRepository.UpdateAsync(link, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to connect link: {ex.Message}");
        }
    }
}

public class AttachDriverToLinkCommandHandler : IRequestHandler<AttachDriverToLinkCommand, IResult<bool>>
{
    private readonly ILinkRepository _linkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDriverFactory _driverFactory;

    public AttachDriverToLinkCommandHandler(
        ILinkRepository linkRepository,
        IUnitOfWork unitOfWork,
        IDriverFactory driverFactory)
    {
        _linkRepository = linkRepository;
        _unitOfWork = unitOfWork;
        _driverFactory = driverFactory;
    }

    public async Task<IResult<bool>> Handle(AttachDriverToLinkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var link = await _linkRepository.GetByIdAsync(request.LinkId, cancellationToken);
            if (link == null)
                return Result<bool>.Failure("Link not found");

            // Create driver configuration
            var driverConfig = new DriverConfiguration(
                connectionString: request.DriverConfiguration.GetValueOrDefault("ConnectionString", "")?.ToString() ?? "",
                parameters: request.DriverConfiguration,
                connectionTimeout: TimeSpan.FromSeconds(30));

            // Create driver
            var driver = await _driverFactory.CreateDriverAsync(request.DriverType, driverConfig, cancellationToken);
            if (driver == null)
                return Result<bool>.Failure($"Failed to create driver of type {request.DriverType}");

            // Attach driver to link
            link.AttachDriver(driver);

            await _linkRepository.UpdateAsync(link, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to attach driver: {ex.Message}");
        }
    }
}

/// <summary>
/// Channel Command Handlers
/// </summary>
public class CreateChannelCommandHandler : IRequestHandler<CreateChannelCommand, IResult<ChannelDto>>
{
    private readonly ILinkRepository _linkRepository;
    private readonly IChannelRepository _channelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateChannelCommandHandler(
        ILinkRepository linkRepository,
        IChannelRepository channelRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _linkRepository = linkRepository;
        _channelRepository = channelRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IResult<ChannelDto>> Handle(CreateChannelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var link = await _linkRepository.GetByIdAsync(request.ChannelDto.LinkId, cancellationToken);
            if (link == null)
                return Result<ChannelDto>.Failure("Link not found");

            var channel = link.AddChannel(
                request.ChannelDto.Name,
                request.ChannelDto.Description,
                request.ChannelDto.ChannelType);

            await _linkRepository.UpdateAsync(link, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var channelDto = _mapper.Map<ChannelDto>(channel);
            return Result<ChannelDto>.Success(channelDto);
        }
        catch (Exception ex)
        {
            return Result<ChannelDto>.Failure($"Failed to create channel: {ex.Message}");
        }
    }
}

/// <summary>
/// DataPoint Command Handlers
/// </summary>
public class CreateDataPointCommandHandler : IRequestHandler<CreateDataPointCommand, IResult<DataPointDto>>
{
    private readonly IChannelRepository _channelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateDataPointCommandHandler(
        IChannelRepository channelRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _channelRepository = channelRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IResult<DataPointDto>> Handle(CreateDataPointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var channel = await _channelRepository.GetByIdAsync(request.DataPointDto.ChannelId, cancellationToken);
            if (channel == null)
                return Result<DataPointDto>.Failure("Channel not found");

            var dataPoint = channel.AddDataPoint(
                request.DataPointDto.Name,
                request.DataPointDto.Description,
                request.DataPointDto.Address,
                request.DataPointDto.DataType,
                request.DataPointDto.AccessMode);

            await _channelRepository.UpdateAsync(channel, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dataPointDto = _mapper.Map<DataPointDto>(dataPoint);
            return Result<DataPointDto>.Success(dataPointDto);
        }
        catch (Exception ex)
        {
            return Result<DataPointDto>.Failure($"Failed to create data point: {ex.Message}");
        }
    }
}

public class ReadDataPointCommandHandler : IRequestHandler<ReadDataPointCommand, IResult<object>>
{
    private readonly IDataPointRepository _dataPointRepository;
    private readonly ILinkRepository _linkRepository;

    public ReadDataPointCommandHandler(
        IDataPointRepository dataPointRepository,
        ILinkRepository linkRepository)
    {
        _dataPointRepository = dataPointRepository;
        _linkRepository = linkRepository;
    }

    public async Task<IResult<object>> Handle(ReadDataPointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dataPoint = await _dataPointRepository.GetByIdAsync(request.DataPointId, cancellationToken);
            if (dataPoint == null)
                return Result<object>.Failure("DataPoint not found");

            // Get the link through the channel
            var channel = await _dataPointRepository.GetChannelByDataPointIdAsync(request.DataPointId, cancellationToken);
            if (channel == null)
                return Result<object>.Failure("Channel not found");

            var link = await _linkRepository.GetByIdAsync(channel.LinkId, cancellationToken);
            if (link == null)
                return Result<object>.Failure("Link not found");

            // Perform read operation through the driver
            var readResult = await link.ReadDataPointAsync(dataPoint, cancellationToken);
            if (readResult.Success)
            {
                return Result<object>.Success(readResult.Value);
            }

            return Result<object>.Failure(readResult.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Result<object>.Failure($"Failed to read data point: {ex.Message}");
        }
    }
}