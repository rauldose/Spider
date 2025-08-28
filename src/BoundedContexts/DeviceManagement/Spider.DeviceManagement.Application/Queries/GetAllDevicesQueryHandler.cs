using MediatR;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DeviceManagement.Application.DTOs;
using Spider.DeviceManagement.Domain.Entities;

namespace Spider.DeviceManagement.Application.Queries;

/// <summary>
/// Handler for GetAllDevicesQuery
/// </summary>
public class GetAllDevicesQueryHandler : IRequestHandler<GetAllDevicesQuery, IEnumerable<DeviceDto>>
{
    private readonly IRepository<Device, Guid> _deviceRepository;

    public GetAllDevicesQueryHandler(IRepository<Device, Guid> deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<IEnumerable<DeviceDto>> Handle(GetAllDevicesQuery request, CancellationToken cancellationToken)
    {
        var devices = await _deviceRepository.GetAllAsync();
        
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