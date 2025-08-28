using System.Linq.Expressions;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DeviceManagement.Domain.Entities;

namespace Spider.DeviceManagement.Application.Specifications;

/// <summary>
/// Specification for finding devices by project ID
/// </summary>
public class DevicesByProjectSpecification : ISpecification<Device>
{
    public DevicesByProjectSpecification(Guid projectId)
    {
        Criteria = device => device.ProjectId == projectId;
        OrderBy = device => device.Name;
    }

    public Expression<Func<Device, bool>>? Criteria { get; }
    public List<Expression<Func<Device, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Device, object>>? OrderBy { get; }
    public Expression<Func<Device, object>>? OrderByDescending { get; }
    public Expression<Func<Device, object>>? GroupBy { get; }
    public int? Take { get; }
    public int? Skip { get; }
    public bool IsPagingEnabled => false;
}