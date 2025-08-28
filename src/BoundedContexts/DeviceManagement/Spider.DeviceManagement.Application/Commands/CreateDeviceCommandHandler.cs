using MediatR;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DeviceManagement.Application.Commands;
using Spider.DeviceManagement.Domain.Entities;
using Spider.DeviceManagement.Domain.Enums;
using Spider.DeviceManagement.Domain.ValueObjects;

namespace Spider.DeviceManagement.Application.Commands;

/// <summary>
/// Handler for creating a new device
/// </summary>
public class CreateDeviceCommandHandler : IRequestHandler<CreateDeviceCommand, Guid>
{
    private readonly IRepository<Device, Guid> _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDeviceCommandHandler(
        IRepository<Device, Guid> deviceRepository,
        IUnitOfWork unitOfWork)
    {
        _deviceRepository = deviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        // Map protocol string to ProtocolType enumeration
        var protocol = ProtocolType.GetAll<ProtocolType>()
            .FirstOrDefault(p => p.Name.Equals(request.Protocol, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Invalid protocol: {request.Protocol}");

        // Create connection parameters
        var connectionParameters = new ConnectionParameters(
            request.Host,
            request.Port,
            request.Timeout,
            request.RetryCount,
            request.AdditionalParameters);

        // Create the device
        var device = new Device(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            protocol,
            connectionParameters,
            request.ProjectId);

        // Save to repository
        await _deviceRepository.AddAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return device.Id;
    }
}