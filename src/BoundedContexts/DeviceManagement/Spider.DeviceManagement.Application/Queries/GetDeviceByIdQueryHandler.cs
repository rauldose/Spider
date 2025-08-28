using MediatR;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DeviceManagement.Application.DTOs;
using Spider.DeviceManagement.Domain.Entities;

namespace Spider.DeviceManagement.Application.Queries;

/// <summary>
/// Handler for GetDeviceByIdQuery
/// </summary>
public class GetDeviceByIdQueryHandler : IRequestHandler<GetDeviceByIdQuery, DeviceDto?>
{
    private readonly IRepository<Device, Guid> _deviceRepository;

    /// <summary>
    /// Initializes a new instance of the GetDeviceByIdQueryHandler
    /// </summary>
    /// <param name="deviceRepository">Device repository</param>
    public GetDeviceByIdQueryHandler(IRepository<Device, Guid> deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    /// <summary>
    /// Handles the GetDeviceByIdQuery
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Device DTO if found, null otherwise</returns>
    public async Task<DeviceDto?> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepository.GetByIdAsync(request.DeviceId, cancellationToken);
        
        if (device == null)
            return null;

        return new DeviceDto
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
        };
    }
}