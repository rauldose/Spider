using MediatR;

namespace Spider.DataAcquisition.Application.Commands;

/// <summary>
/// Command to create a new data point
/// </summary>
public record CreateDataPointCommand(
    string Name,
    string Address,
    string DataType,
    Guid DeviceId,
    string? Description = null,
    int ScanInterval = 1000,
    string? Group = null,
    int? Register = null) : IRequest<Guid>;