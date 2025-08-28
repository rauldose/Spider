using MediatR;
using Microsoft.AspNetCore.Mvc;
using Spider.Communication.Application.Commands;
using Spider.Communication.Application.Queries;
using Spider.Communication.Application.DTOs;

namespace Spider.Communication.API.Controllers;

/// <summary>
/// Controller for Channel management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChannelsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChannelsController> _logger;

    public ChannelsController(IMediator mediator, ILogger<ChannelsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get channel by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChannelDto>> GetChannel(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetChannelByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return NotFound(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel {ChannelId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get channels by link ID
    /// </summary>
    [HttpGet("link/{linkId:guid}")]
    public async Task<ActionResult<IEnumerable<ChannelDto>>> GetChannelsByLink(Guid linkId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetChannelsByLinkIdQuery(linkId);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channels for link {LinkId}", linkId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all active channels
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ChannelDto>>> GetActiveChannels(CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetActiveChannelsQuery();
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active channels");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new channel
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ChannelDto>> CreateChannel(
        [FromBody] CreateChannelDto channelDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new CreateChannelCommand(channelDto);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetChannel), new { id = result.Value!.Id }, result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating channel");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a channel
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ChannelDto>> UpdateChannel(
        Guid id,
        [FromBody] UpdateChannelRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UpdateChannelCommand(id, request.Name, request.Description, request.ChannelType);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel {ChannelId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a channel
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteChannel(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeleteChannelCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting channel {ChannelId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Enable a channel
    /// </summary>
    [HttpPost("{id:guid}/enable")]
    public async Task<ActionResult> EnableChannel(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EnableChannelCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(new { message = "Channel enabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling channel {ChannelId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Disable a channel
    /// </summary>
    [HttpPost("{id:guid}/disable")]
    public async Task<ActionResult> DisableChannel(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DisableChannelCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(new { message = "Channel disabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling channel {ChannelId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
/// Controller for DataPoint management operations
/// </summary>
[ApiController]
[Route("api/datapoints")]
[Produces("application/json")]
public class DataPointsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DataPointsController> _logger;

    public DataPointsController(IMediator mediator, ILogger<DataPointsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get data point by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DataPointDto>> GetDataPoint(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetDataPointByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return NotFound(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data point {DataPointId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get data points by channel ID
    /// </summary>
    [HttpGet("channel/{channelId:guid}")]
    public async Task<ActionResult<IEnumerable<DataPointDto>>> GetDataPointsByChannel(Guid channelId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetDataPointsByChannelIdQuery(channelId);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data points for channel {ChannelId}", channelId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get data points by link ID
    /// </summary>
    [HttpGet("link/{linkId:guid}")]
    public async Task<ActionResult<IEnumerable<DataPointDto>>> GetDataPointsByLink(Guid linkId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetDataPointsByLinkIdQuery(linkId);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data points for link {LinkId}", linkId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new data point
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DataPointDto>> CreateDataPoint(
        [FromBody] CreateDataPointDto dataPointDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new CreateDataPointCommand(dataPointDto);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetDataPoint), new { id = result.Value!.Id }, result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating data point");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Read data point value
    /// </summary>
    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<object>> ReadDataPoint(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ReadDataPointCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(new { value = result.Value, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading data point {DataPointId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Write data point value
    /// </summary>
    [HttpPost("{id:guid}/write")]
    public async Task<ActionResult> WriteDataPoint(
        Guid id,
        [FromBody] WriteDataPointRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new WriteDataPointCommand(id, request.Value);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(new { message = "Data point written successfully", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing data point {DataPointId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Bulk read data points
    /// </summary>
    [HttpPost("bulk-read")]
    public async Task<ActionResult<Dictionary<Guid, object>>> BulkReadDataPoints(
        [FromBody] BulkReadRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new BulkReadDataPointsCommand(request.DataPointIds);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk reading data points");
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
/// Request models
/// </summary>
public record UpdateChannelRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ChannelType { get; init; } = string.Empty;
}

public record WriteDataPointRequest
{
    public object Value { get; init; } = new();
}

public record BulkReadRequest
{
    public List<Guid> DataPointIds { get; init; } = new();
}