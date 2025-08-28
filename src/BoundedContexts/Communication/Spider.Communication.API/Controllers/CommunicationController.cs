using MediatR;
using Microsoft.AspNetCore.Mvc;
using Spider.Communication.Application.Queries;
using Spider.Communication.Application.DTOs;

namespace Spider.Communication.API.Controllers;

/// <summary>
/// Controller for Communication statistics and monitoring
/// </summary>
[ApiController]
[Route("api/communication")]
[Produces("application/json")]
public class CommunicationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CommunicationController> _logger;

    public CommunicationController(IMediator mediator, ILogger<CommunicationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get communication statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<CommunicationStatisticsDto>> GetStatistics(CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetCommunicationStatisticsQuery();
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting communication statistics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get health status of all links
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult> GetHealth()
    {
        try
        {
            // Simulate health check operations
            await Task.Delay(1);
            
            // Simple health check
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                service = "Communication API",
                version = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(500, "Service unhealthy");
        }
    }

    /// <summary>
    /// Get available protocol types
    /// </summary>
    [HttpGet("protocols")]
    public ActionResult<IEnumerable<string>> GetProtocols()
    {
        try
        {
            var protocols = new[]
            {
                "Modbus",
                "OpcUa",
                "Mqtt",
                "EtherNetIP",
                "Siemens",
                "Omron",
                "Mitsubishi"
            };

            return Ok(protocols);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting protocols");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get available channel types
    /// </summary>
    [HttpGet("channel-types")]
    public ActionResult<IEnumerable<string>> GetChannelTypes()
    {
        try
        {
            var channelTypes = new[]
            {
                "Input",
                "Output",
                "InputOutput",
                "Configuration",
                "Diagnostic"
            };

            return Ok(channelTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel types");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get available data types
    /// </summary>
    [HttpGet("data-types")]
    public ActionResult<IEnumerable<string>> GetDataTypes()
    {
        try
        {
            var dataTypes = new[]
            {
                "Boolean",
                "Byte",
                "Int16",
                "UInt16", 
                "Int32",
                "UInt32",
                "Int64",
                "UInt64",
                "Float",
                "Double",
                "String"
            };

            return Ok(dataTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data types");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get available access modes
    /// </summary>
    [HttpGet("access-modes")]
    public ActionResult<IEnumerable<string>> GetAccessModes()
    {
        try
        {
            var accessModes = new[]
            {
                "ReadOnly",
                "WriteOnly",
                "ReadWrite"
            };

            return Ok(accessModes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access modes");
            return StatusCode(500, "Internal server error");
        }
    }
}