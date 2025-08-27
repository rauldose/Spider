using MediatR;

namespace Spider.Core.SharedKernel.Abstractions;

/// <summary>
/// Represents a domain event that can be published and handled
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// The timestamp when the event occurred
    /// </summary>
    DateTimeOffset OccurredOn { get; }
    
    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    Guid EventId { get; }
}