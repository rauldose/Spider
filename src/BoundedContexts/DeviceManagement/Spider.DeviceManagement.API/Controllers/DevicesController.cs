using Microsoft.AspNetCore.Mvc;
using MediatR;
using Spider.DeviceManagement.Application.Commands;
using Spider.DeviceManagement.Application.Queries;
using Spider.DeviceManagement.Application.DTOs;

namespace Spider.DeviceManagement.API.Controllers;

/// <summary>
/// API controller for device management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the DevicesController
    /// </summary>
    /// <param name="mediator">The mediator instance for CQRS operations</param>
    public DevicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new device
    /// </summary>
    /// <param name="command">Device creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ID of the created device</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateDevice(
        [FromBody] CreateDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        var deviceId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetDevicesByProject), new { projectId = command.ProjectId }, deviceId);
    }

    /// <summary>
    /// Gets all devices for a specific project
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of devices in the project</returns>
    [HttpGet("project/{projectId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<DeviceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetDevicesByProject(
        [FromRoute] Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDevicesByProjectQuery(projectId);
        var devices = await _mediator.Send(query, cancellationToken);
        return Ok(devices);
    }

    /// <summary>
    /// Gets all available protocol types
    /// </summary>
    /// <returns>List of supported protocol types</returns>
    [HttpGet("protocols")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetProtocolTypes()
    {
        var protocols = new[]
        {
            "Modbus",
            "OPC UA",
            "MQTT",
            "TCP/IP",
            "Serial Port",
            "Ethernet",
            "CAN Bus",
            "PROFINET",
            "EtherCAT",
            "Custom"
        };
        
        return Ok(protocols);
    }

    /// <summary>
    /// Health check endpoint for device management service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", service = "device-management", timestamp = DateTimeOffset.UtcNow });
    }
}