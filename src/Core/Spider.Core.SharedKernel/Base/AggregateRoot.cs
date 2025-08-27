using Spider.Core.SharedKernel.Abstractions;

namespace Spider.Core.SharedKernel.Base;

/// <summary>
/// Base aggregate root class that manages domain events and entity boundaries
/// </summary>
/// <typeparam name="TId">The type of the aggregate root ID</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
{
    protected AggregateRoot() : base() { }

    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Publishes a domain event from this aggregate root
    /// </summary>
    protected void PublishDomainEvent(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }

    /// <summary>
    /// Version for optimistic concurrency control
    /// </summary>
    public long Version { get; protected set; }

    /// <summary>
    /// Increments the version for optimistic concurrency control
    /// </summary>
    protected void IncrementVersion()
    {
        Version++;
    }
}