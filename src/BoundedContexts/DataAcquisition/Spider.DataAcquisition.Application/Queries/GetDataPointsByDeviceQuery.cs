using MediatR;
using Spider.DataAcquisition.Application.DTOs;

namespace Spider.DataAcquisition.Application.Queries;

/// <summary>
/// Query to get all data points for a device
/// </summary>
public record GetDataPointsByDeviceQuery(Guid DeviceId) : IRequest<IEnumerable<DataPointDto>>;