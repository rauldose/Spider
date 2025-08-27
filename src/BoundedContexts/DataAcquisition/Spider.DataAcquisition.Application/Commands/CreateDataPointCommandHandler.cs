using MediatR;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DataAcquisition.Domain.Entities;
using Spider.DataAcquisition.Domain.Enumerations;
using Spider.DataAcquisition.Domain.ValueObjects;

namespace Spider.DataAcquisition.Application.Commands;

/// <summary>
/// Handler for creating new data points
/// </summary>
public class CreateDataPointCommandHandler : IRequestHandler<CreateDataPointCommand, Guid>
{
    private readonly IRepository<DataPoint, Guid> _dataPointRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDataPointCommandHandler(
        IRepository<DataPoint, Guid> dataPointRepository,
        IUnitOfWork unitOfWork)
    {
        _dataPointRepository = dataPointRepository ?? throw new ArgumentNullException(nameof(dataPointRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Guid> Handle(CreateDataPointCommand request, CancellationToken cancellationToken)
    {
        // Parse data type
        var dataType = DataType.GetAll().FirstOrDefault(dt => 
            string.Equals(dt.Name, request.DataType, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Invalid data type: {request.DataType}");

        // Create address value object
        var address = new DataAddress(request.Address, request.Group, request.Register);

        // Create data point
        var dataPoint = new DataPoint(
            Guid.NewGuid(),
            request.Name,
            address,
            dataType,
            request.DeviceId,
            request.Description,
            request.ScanInterval);

        await _dataPointRepository.AddAsync(dataPoint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return dataPoint.Id;
    }
}