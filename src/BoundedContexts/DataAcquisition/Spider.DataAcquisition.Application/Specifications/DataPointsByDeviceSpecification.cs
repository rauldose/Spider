using Spider.Core.SharedKernel.Abstractions;
using Spider.DataAcquisition.Domain.Entities;
using System.Linq.Expressions;

namespace Spider.DataAcquisition.Application.Specifications;

/// <summary>
/// Specification for finding data points by device
/// </summary>
public class DataPointsByDeviceSpecification : ISpecification<DataPoint>
{
    private readonly Guid _deviceId;

    public DataPointsByDeviceSpecification(Guid deviceId)
    {
        _deviceId = deviceId;
    }

    public Expression<Func<DataPoint, bool>> Criteria => dp => dp.DeviceId == _deviceId;
    public List<Expression<Func<DataPoint, object>>> Includes => new();
    public List<string> IncludeStrings => new();
    public Expression<Func<DataPoint, object>>? OrderBy => dp => dp.Name;
    public Expression<Func<DataPoint, object>>? OrderByDescending => null;
    public Expression<Func<DataPoint, object>>? GroupBy => null;
    public int? Take => null;
    public int? Skip => null;
    public bool IsPagingEnabled => false;
}