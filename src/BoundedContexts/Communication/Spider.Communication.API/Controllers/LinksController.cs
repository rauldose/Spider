using MediatR;
using Microsoft.AspNetCore.Mvc;
using Spider.Communication.Application.Commands;
using Spider.Communication.Application.Queries;
using Spider.Communication.Application.DTOs;

namespace Spider.Communication.API.Controllers;

/// <summary>
/// Controller for Link management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LinksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LinksController> _logger;

    public LinksController(IMediator mediator, ILogger<LinksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all links with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LinkDto>>> GetLinks(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetAllLinksQuery(page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            Response.Headers.Append("X-Total-Count", result.Value!.TotalCount.ToString());
            Response.Headers.Append("X-Page", result.Value.Page.ToString());
            Response.Headers.Append("X-Page-Size", result.Value.PageSize.ToString());
            Response.Headers.Append("X-Total-Pages", result.Value.TotalPages.ToString());

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting links");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get link by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LinkDto>> GetLink(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetLinkByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return NotFound(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting link {LinkId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get links by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<LinkDto>>> GetLinksByStatus(
        string status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetLinksByStatusQuery(status, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            Response.Headers.Append("X-Total-Count", result.Value!.TotalCount.ToString());
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting links by status {Status}", status);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new link
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<LinkDto>> CreateLink(
        [FromBody] CreateLinkDto linkDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new CreateLinkCommand(linkDto);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetLink), new { id = result.Value!.Id }, result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating link");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a link
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LinkDto>> UpdateLink(
        Guid id,
        [FromBody] UpdateLinkDto linkDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != linkDto.Id)
                return BadRequest("ID mismatch");

            var command = new UpdateLinkCommand(linkDto);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating link {LinkId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a link
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteLink(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeleteLinkCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting link {LinkId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Connect a link
    /// </summary>
    [HttpPost("{id:guid}/connect")]
    public async Task<ActionResult> ConnectLink(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ConnectLinkCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(new { message = "Link connected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting link {LinkId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Disconnect a link
    /// </summary>
    [HttpPost("{id:guid}/disconnect")]
    public async Task<ActionResult> DisconnectLink(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DisconnectLinkCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(new { message = "Link disconnected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting link {LinkId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Attach driver to link
    /// </summary>
    [HttpPost("{id:guid}/attach-driver")]
    public async Task<ActionResult> AttachDriver(
        Guid id,
        [FromBody] AttachDriverRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new AttachDriverToLinkCommand(id, request.DriverType, request.Configuration);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(new { message = "Driver attached successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attaching driver to link {LinkId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get link health
    /// </summary>
    [HttpGet("{id:guid}/health")]
    public async Task<ActionResult<LinkHealthDto>> GetLinkHealth(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetLinkHealthQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return NotFound(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting link health {LinkId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Diagnose link
    /// </summary>
    [HttpPost("{id:guid}/diagnose")]
    public async Task<ActionResult<Dictionary<string, object>>> DiagnoseLink(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DiagnoseLinkCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error diagnosing link {LinkId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
/// Request model for attaching driver
/// </summary>
public record AttachDriverRequest
{
    public string DriverType { get; init; } = string.Empty;
    public Dictionary<string, object> Configuration { get; init; } = new();
}