using Microsoft.AspNetCore.Mvc;
using MediatR;
using Spider.DataAcquisition.Application.Commands;
using Spider.DataAcquisition.Application.Queries;
using Spider.DataAcquisition.Application.DTOs;

namespace Spider.DataAcquisition.API.Controllers;

/// <summary>
/// API controller for data point management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DataPointsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DataPointsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Creates a new data point
    /// </summary>
    /// <param name="command">The data point creation command</param>
    /// <returns>The ID of the created data point</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateDataPoint([FromBody] CreateDataPointCommand command)
    {
        var dataPointId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDataPointsByDevice), new { deviceId = command.DeviceId }, dataPointId);
    }

    /// <summary>
    /// Gets all data points for a specific device
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>List of data points</returns>
    [HttpGet("device/{deviceId}")]
    [ProducesResponseType(typeof(IEnumerable<DataPointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DataPointDto>>> GetDataPointsByDevice(Guid deviceId)
    {
        var query = new GetDataPointsByDeviceQuery(deviceId);
        var dataPoints = await _mediator.Send(query);
        return Ok(dataPoints);
    }

    /// <summary>
    /// Updates a data point's value
    /// </summary>
    /// <param name="dataPointId">The data point ID</param>
    /// <param name="command">The value update command</param>
    /// <returns>No content</returns>
    [HttpPut("{dataPointId}/value")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDataPointValue(Guid dataPointId, [FromBody] UpdateDataPointValueCommand command)
    {
        var updatedCommand = command with { DataPointId = dataPointId };
        await _mediator.Send(updatedCommand);
        return NoContent();
    }

    /// <summary>
    /// Gets health status of the Data Acquisition service
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "healthy", service = "DataAcquisition", timestamp = DateTime.UtcNow });
    }
}