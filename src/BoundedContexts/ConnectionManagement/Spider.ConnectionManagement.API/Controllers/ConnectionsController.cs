using MediatR;
using Microsoft.AspNetCore.Mvc;
using Spider.ConnectionManagement.Application.Commands;
using Spider.ConnectionManagement.Application.Queries;
using Spider.ConnectionManagement.Application.DTOs;

namespace Spider.ConnectionManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConnectionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConnectionsController> _logger;

    public ConnectionsController(IMediator mediator, ILogger<ConnectionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all connections
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConnectionDto>>> GetAllConnections()
    {
        var connections = await _mediator.Send(new GetAllConnectionsQuery());
        return Ok(connections);
    }

    /// <summary>
    /// Get connection by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConnectionDto>> GetConnection(Guid id)
    {
        var connection = await _mediator.Send(new GetConnectionByIdQuery(id));
        
        if (connection == null)
            return NotFound($"Connection with ID {id} not found");

        return Ok(connection);
    }

    /// <summary>
    /// Get connections by device ID
    /// </summary>
    [HttpGet("device/{deviceId:guid}")]
    public async Task<ActionResult<IEnumerable<ConnectionDto>>> GetConnectionsByDevice(Guid deviceId)
    {
        var connections = await _mediator.Send(new GetConnectionsByDeviceIdQuery(deviceId));
        return Ok(connections);
    }

    /// <summary>
    /// Get connections by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<ConnectionDto>>> GetConnectionsByStatus(string status)
    {
        var connections = await _mediator.Send(new GetConnectionsByStatusQuery(status));
        return Ok(connections);
    }

    /// <summary>
    /// Get unhealthy connections
    /// </summary>
    [HttpGet("unhealthy")]
    public async Task<ActionResult<IEnumerable<ConnectionDto>>> GetUnhealthyConnections()
    {
        var connections = await _mediator.Send(new GetUnhealthyConnectionsQuery());
        return Ok(connections);
    }

    /// <summary>
    /// Get connection statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ConnectionStatisticsDto>> GetConnectionStatistics()
    {
        var statistics = await _mediator.Send(new GetConnectionStatisticsQuery());
        return Ok(statistics);
    }

    /// <summary>
    /// Get available protocols
    /// </summary>
    [HttpGet("protocols")]
    public async Task<ActionResult<IEnumerable<string>>> GetAvailableProtocols()
    {
        var protocols = await _mediator.Send(new GetAvailableProtocolsQuery());
        return Ok(protocols);
    }

    /// <summary>
    /// Create a new connection
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ConnectionDto>> CreateConnection([FromBody] CreateConnectionDto dto)
    {
        var command = new CreateConnectionCommand(
            dto.DeviceId,
            dto.Name,
            dto.Protocol,
            dto.Host,
            dto.Port,
            dto.TimeoutMs,
            dto.RetryAttempts,
            dto.ExtendedProperties);

        var connection = await _mediator.Send(command);
        
        return CreatedAtAction(
            nameof(GetConnection), 
            new { id = connection.Id }, 
            connection);
    }

    /// <summary>
    /// Update connection parameters
    /// </summary>
    [HttpPut("{id:guid}/parameters")]
    public async Task<ActionResult<ConnectionDto>> UpdateConnectionParameters(
        Guid id, 
        [FromBody] UpdateConnectionParametersDto dto)
    {
        var command = new UpdateConnectionParametersCommand(
            id,
            dto.Host,
            dto.Port,
            dto.TimeoutMs,
            dto.RetryAttempts,
            dto.ExtendedProperties);

        var connection = await _mediator.Send(command);
        return Ok(connection);
    }

    /// <summary>
    /// Connect a connection
    /// </summary>
    [HttpPost("{id:guid}/connect")]
    public async Task<ActionResult<bool>> Connect(Guid id)
    {
        var result = await _mediator.Send(new ConnectCommand(id));
        
        if (result)
            return Ok(new { success = true, message = "Connection established" });
        
        return BadRequest(new { success = false, message = "Failed to establish connection" });
    }

    /// <summary>
    /// Disconnect a connection
    /// </summary>
    [HttpPost("{id:guid}/disconnect")]
    public async Task<ActionResult<bool>> Disconnect(Guid id, [FromBody] DisconnectRequest? request = null)
    {
        var result = await _mediator.Send(new DisconnectCommand(id, request?.Reason));
        
        if (result)
            return Ok(new { success = true, message = "Connection disconnected" });
        
        return BadRequest(new { success = false, message = "Failed to disconnect connection" });
    }

    /// <summary>
    /// Test connection parameters
    /// </summary>
    [HttpPost("test")]
    public async Task<ActionResult<bool>> TestConnection([FromBody] TestConnectionRequest request)
    {
        var command = new TestConnectionCommand(
            request.Protocol,
            request.Host,
            request.Port,
            request.TimeoutMs,
            request.ExtendedProperties);

        var result = await _mediator.Send(command);
        
        return Ok(new { 
            success = result, 
            message = result ? "Connection test successful" : "Connection test failed" 
        });
    }

    /// <summary>
    /// Delete a connection
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteConnection(Guid id)
    {
        var result = await _mediator.Send(new DeleteConnectionCommand(id));
        
        if (result)
            return NoContent();
        
        return NotFound($"Connection with ID {id} not found");
    }

    /// <summary>
    /// Get connection health
    /// </summary>
    [HttpGet("{id:guid}/health")]
    public async Task<ActionResult<ConnectionHealthDto>> GetConnectionHealth(Guid id)
    {
        var health = await _mediator.Send(new GetConnectionHealthQuery(id));
        
        if (health == null)
            return NotFound($"Connection with ID {id} not found");

        return Ok(health);
    }

    /// <summary>
    /// Get connection status
    /// </summary>
    [HttpGet("{id:guid}/status")]
    public async Task<ActionResult<ConnectionStatusDto>> GetConnectionStatus(Guid id)
    {
        var status = await _mediator.Send(new GetConnectionStatusQuery(id));
        
        if (status == null)
            return NotFound($"Connection with ID {id} not found");

        return Ok(status);
    }
}

public record DisconnectRequest(string? Reason);

public record TestConnectionRequest(
    string Protocol,
    string Host,
    int Port,
    int TimeoutMs = 5000,
    Dictionary<string, object>? ExtendedProperties = null);