using MediatR;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DeviceManagement.Application.DTOs;
using Spider.DeviceManagement.Application.Queries;
using Spider.DeviceManagement.Application.Specifications;
using Spider.DeviceManagement.Domain.Entities;

namespace Spider.DeviceManagement.Application.Queries;

/// <summary>
/// Handler for getting devices by project query
/// </summary>
public class GetDevicesByProjectQueryHandler : IRequestHandler<GetDevicesByProjectQuery, IEnumerable<DeviceDto>>
{
    private readonly IRepository<Device, Guid> _deviceRepository;

    public GetDevicesByProjectQueryHandler(IRepository<Device, Guid> deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<IEnumerable<DeviceDto>> Handle(GetDevicesByProjectQuery request, CancellationToken cancellationToken)
    {
        var devices = await _deviceRepository.FindAsync(
            new DevicesByProjectSpecification(request.ProjectId), 
            cancellationToken);

        return devices.Select(device => new DeviceDto
        {
            Id = device.Id,
            Name = device.Name,
            Description = device.Description,
            Protocol = device.Protocol.Name,
            Status = device.Status.Name,
            ConnectionString = device.ConnectionParameters.GetConnectionString(),
            LastError = device.LastError,
            LastConnectedAt = device.LastConnectedAt,
            ProjectId = device.ProjectId,
            IsEnabled = device.IsEnabled,
            CreatedAt = device.CreatedAt,
            UpdatedAt = device.UpdatedAt
        });
    }
}