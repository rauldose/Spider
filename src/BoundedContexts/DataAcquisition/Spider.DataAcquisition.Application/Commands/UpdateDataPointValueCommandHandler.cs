using MediatR;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DataAcquisition.Domain.Entities;
using Spider.DataAcquisition.Domain.Enumerations;
using Spider.DataAcquisition.Domain.ValueObjects;

namespace Spider.DataAcquisition.Application.Commands;

/// <summary>
/// Handler for updating data point values
/// </summary>
public class UpdateDataPointValueCommandHandler : IRequestHandler<UpdateDataPointValueCommand>
{
    private readonly IRepository<DataPoint, Guid> _dataPointRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDataPointValueCommandHandler(
        IRepository<DataPoint, Guid> dataPointRepository,
        IUnitOfWork unitOfWork)
    {
        _dataPointRepository = dataPointRepository ?? throw new ArgumentNullException(nameof(dataPointRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(UpdateDataPointValueCommand request, CancellationToken cancellationToken)
    {
        var dataPoint = await _dataPointRepository.GetByIdAsync(request.DataPointId, cancellationToken)
            ?? throw new ArgumentException($"Data point not found: {request.DataPointId}");

        // Parse quality
        var quality = DataQuality.GetAll().FirstOrDefault(q => 
            string.Equals(q.Name, request.Quality, StringComparison.OrdinalIgnoreCase))
            ?? DataQuality.Good;

        // Create data value
        var dataValue = new DataValue(request.Value, dataPoint.DataType, quality, DateTime.UtcNow);

        // Update data point
        dataPoint.UpdateValue(dataValue);

        await _dataPointRepository.UpdateAsync(dataPoint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}