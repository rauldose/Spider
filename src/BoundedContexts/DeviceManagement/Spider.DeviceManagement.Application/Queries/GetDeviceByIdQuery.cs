using MediatR;
using Spider.DeviceManagement.Application.DTOs;

namespace Spider.DeviceManagement.Application.Queries;

/// <summary>
/// Query to get a device by its ID
/// </summary>
/// <param name="DeviceId">The device identifier</param>
public record GetDeviceByIdQuery(Guid DeviceId) : IRequest<DeviceDto?>;