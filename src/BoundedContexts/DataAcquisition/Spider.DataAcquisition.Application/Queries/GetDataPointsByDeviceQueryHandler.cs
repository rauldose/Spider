using MediatR;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DataAcquisition.Application.DTOs;
using Spider.DataAcquisition.Application.Specifications;
using Spider.DataAcquisition.Domain.Entities;

namespace Spider.DataAcquisition.Application.Queries;

/// <summary>
/// Handler for getting data points by device
/// </summary>
public class GetDataPointsByDeviceQueryHandler : IRequestHandler<GetDataPointsByDeviceQuery, IEnumerable<DataPointDto>>
{
    private readonly IRepository<DataPoint, Guid> _dataPointRepository;

    public GetDataPointsByDeviceQueryHandler(IRepository<DataPoint, Guid> dataPointRepository)
    {
        _dataPointRepository = dataPointRepository ?? throw new ArgumentNullException(nameof(dataPointRepository));
    }

    public async Task<IEnumerable<DataPointDto>> Handle(GetDataPointsByDeviceQuery request, CancellationToken cancellationToken)
    {
        var spec = new DataPointsByDeviceSpecification(request.DeviceId);
        var dataPoints = await _dataPointRepository.FindAsync(spec, cancellationToken);

        return dataPoints.Select(dp => new DataPointDto(
            dp.Id,
            dp.Name,
            dp.Description,
            dp.Address.Address,
            dp.DataType.Name,
            dp.DeviceId,
            dp.IsEnabled,
            dp.ScanInterval,
            dp.LastValue?.Value,
            dp.LastValue?.Quality.Name,
            dp.LastScanTime));
    }
}