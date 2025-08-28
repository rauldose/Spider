using Spider.Core.SharedKernel.Abstractions;
using Spider.ConnectionManagement.Domain.Entities;
using System.Linq.Expressions;

namespace Spider.ConnectionManagement.Domain.Specifications;

public class ConnectionByDeviceIdSpecification : ISpecification<Connection>
{
    public ConnectionByDeviceIdSpecification(Guid deviceId)
    {
        Criteria = connection => connection.DeviceId == deviceId;
        OrderBy = connection => connection.Name;
    }

    public Expression<Func<Connection, bool>>? Criteria { get; }
    public List<Expression<Func<Connection, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Connection, object>>? OrderBy { get; }
    public Expression<Func<Connection, object>>? OrderByDescending { get; }
    public Expression<Func<Connection, object>>? GroupBy { get; }
    public int? Take { get; }
    public int? Skip { get; }
    public bool IsPagingEnabled => false;
}

public class ConnectionByStatusSpecification : ISpecification<Connection>
{
    public ConnectionByStatusSpecification(string statusName)
    {
        Criteria = connection => connection.Status.Name == statusName;
        OrderBy = connection => connection.Name;
    }

    public Expression<Func<Connection, bool>>? Criteria { get; }
    public List<Expression<Func<Connection, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Connection, object>>? OrderBy { get; }
    public Expression<Func<Connection, object>>? OrderByDescending { get; }
    public Expression<Func<Connection, object>>? GroupBy { get; }
    public int? Take { get; }
    public int? Skip { get; }
    public bool IsPagingEnabled => false;
}

public class UnhealthyConnectionsSpecification : ISpecification<Connection>
{
    public UnhealthyConnectionsSpecification()
    {
        Criteria = connection => !connection.Health.IsHealthy;
        OrderByDescending = connection => connection.Health.ConsecutiveFailures;
    }

    public Expression<Func<Connection, bool>>? Criteria { get; }
    public List<Expression<Func<Connection, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Connection, object>>? OrderBy { get; }
    public Expression<Func<Connection, object>>? OrderByDescending { get; }
    public Expression<Func<Connection, object>>? GroupBy { get; }
    public int? Take { get; }
    public int? Skip { get; }
    public bool IsPagingEnabled => false;
}

public class ConnectionsNeedingReconnectionSpecification : ISpecification<Connection>
{
    public ConnectionsNeedingReconnectionSpecification()
    {
        Criteria = connection => 
            (connection.Status.Name == "Failed" || connection.Status.Name == "TimedOut") &&
            connection.Health.ConsecutiveFailures < 5; // Don't retry if too many failures
        OrderBy = connection => connection.Health.ConsecutiveFailures;
    }

    public Expression<Func<Connection, bool>>? Criteria { get; }
    public List<Expression<Func<Connection, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Connection, object>>? OrderBy { get; }
    public Expression<Func<Connection, object>>? OrderByDescending { get; }
    public Expression<Func<Connection, object>>? GroupBy { get; }
    public int? Take { get; }
    public int? Skip { get; }
    public bool IsPagingEnabled => false;
}