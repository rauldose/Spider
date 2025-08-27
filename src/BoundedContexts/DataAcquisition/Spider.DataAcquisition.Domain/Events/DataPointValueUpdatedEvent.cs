using Spider.Core.SharedKernel.Abstractions;
using Spider.DataAcquisition.Domain.ValueObjects;

namespace Spider.DataAcquisition.Domain.Events;

/// <summary>
/// Domain event fired when a data point value is updated
/// </summary>
public record DataPointValueUpdatedEvent(
    Guid DataPointId, 
    string DataPointName, 
    DataValue? PreviousValue, 
    DataValue NewValue) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}