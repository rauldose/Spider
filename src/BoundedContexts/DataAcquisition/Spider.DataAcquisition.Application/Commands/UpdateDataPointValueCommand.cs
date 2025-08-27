using MediatR;

namespace Spider.DataAcquisition.Application.Commands;

/// <summary>
/// Command to update a data point's value
/// </summary>
public record UpdateDataPointValueCommand(
    Guid DataPointId,
    object Value,
    string Quality = "Good") : IRequest;