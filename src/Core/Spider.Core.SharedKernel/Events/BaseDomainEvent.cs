using Spider.Core.SharedKernel.Abstractions;

namespace Spider.Core.SharedKernel.Events;

/// <summary>
/// Base implementation of domain event
/// </summary>
public abstract class BaseDomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; }
    public Guid EventId { get; }

    protected BaseDomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
    }
}