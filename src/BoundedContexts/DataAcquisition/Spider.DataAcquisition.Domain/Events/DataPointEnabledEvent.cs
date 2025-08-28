using Spider.Core.SharedKernel.Abstractions;

namespace Spider.DataAcquisition.Domain.Events;

/// <summary>
/// Domain event fired when a data point is enabled
/// </summary>
public record DataPointEnabledEvent(Guid DataPointId, string DataPointName) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}