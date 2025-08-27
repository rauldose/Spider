using Spider.Core.SharedKernel.Abstractions;
using Spider.Communication.Domain.ValueObjects;

namespace Spider.Communication.Domain.Events;

// Base implementation for domain events
public abstract record CommunicationDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

// Link Events
public record LinkCreatedEvent(Guid LinkId, string Name, string ProtocolType) : CommunicationDomainEvent;

public record LinkDriverAttachedEvent(Guid LinkId, string DriverName, string? PreviousDriverName) : CommunicationDomainEvent;

public record LinkDriverDetachedEvent(Guid LinkId, string DriverName) : CommunicationDomainEvent;

public record LinkStatusChangedEvent(Guid LinkId, string PreviousStatus, string CurrentStatus, string? Reason) : CommunicationDomainEvent;

public record LinkErrorOccurredEvent(Guid LinkId, string ErrorCode, string ErrorMessage) : CommunicationDomainEvent;

public record LinkChannelAddedEvent(Guid LinkId, Guid ChannelId, string ChannelName) : CommunicationDomainEvent;

public record LinkChannelRemovedEvent(Guid LinkId, Guid ChannelId, string ChannelName) : CommunicationDomainEvent;

public record LinkConfigurationUpdatedEvent(Guid LinkId, LinkConfiguration PreviousConfiguration, LinkConfiguration NewConfiguration) : CommunicationDomainEvent;

public record LinkMetadataUpdatedEvent(Guid LinkId, LinkMetadata PreviousMetadata, LinkMetadata NewMetadata) : CommunicationDomainEvent;

// Channel Events
public record ChannelCreatedEvent(Guid ChannelId, string Name, string Type) : CommunicationDomainEvent;

public record ChannelAssignedToLinkEvent(Guid ChannelId, string ChannelName, Guid LinkId) : CommunicationDomainEvent;

public record ChannelUnassignedFromLinkEvent(Guid ChannelId, string ChannelName, Guid PreviousLinkId) : CommunicationDomainEvent;

public record ChannelStatusChangedEvent(Guid ChannelId, string ChannelName, string PreviousStatus, string CurrentStatus, string? Reason) : CommunicationDomainEvent;

public record ChannelDataPointAddedEvent(Guid ChannelId, Guid DataPointId, string Address) : CommunicationDomainEvent;

public record ChannelDataPointRemovedEvent(Guid ChannelId, Guid DataPointId, string Address) : CommunicationDomainEvent;

public record ChannelDataReadEvent(Guid ChannelId, string Address, object? Value, DateTime Timestamp) : CommunicationDomainEvent;

public record ChannelDataWrittenEvent(Guid ChannelId, string Address, object Value, DateTime Timestamp) : CommunicationDomainEvent;

public record ChannelErrorOccurredEvent(Guid ChannelId, string ErrorCode, string ErrorMessage) : CommunicationDomainEvent;

public record ChannelConfigurationUpdatedEvent(Guid ChannelId, string ChannelName, ChannelConfiguration PreviousConfiguration, ChannelConfiguration NewConfiguration) : CommunicationDomainEvent;

public record ChannelRenamedEvent(Guid ChannelId, string PreviousName, string NewName, string PreviousDescription, string NewDescription) : CommunicationDomainEvent;