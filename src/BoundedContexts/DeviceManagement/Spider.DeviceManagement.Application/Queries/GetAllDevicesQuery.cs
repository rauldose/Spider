using MediatR;
using Spider.DeviceManagement.Application.DTOs;

namespace Spider.DeviceManagement.Application.Queries;

/// <summary>
/// Query to get all devices
/// </summary>
public record GetAllDevicesQuery : IRequest<IEnumerable<DeviceDto>>;