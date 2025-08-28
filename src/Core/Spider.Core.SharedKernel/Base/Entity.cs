using Spider.Core.SharedKernel.Abstractions;

namespace Spider.Core.SharedKernel.Base;

/// <summary>
/// Base entity class with common properties and domain event support
/// </summary>
/// <typeparam name="TId">The type of the entity ID</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// The unique identifier for this entity
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// When the entity was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; protected set; }

    /// <summary>
    /// When the entity was last updated
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; protected set; }

    /// <summary>
    /// Who created this entity
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// Who last updated this entity
    /// </summary>
    public string? UpdatedBy { get; protected set; }

    /// <summary>
    /// Domain events raised by this entity
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity()
    {
        CreatedAt = DateTimeOffset.UtcNow;
    }

    protected Entity(TId id) : this()
    {
        Id = id;
    }

    /// <summary>
    /// Adds a domain event to be published
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event
    /// </summary>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marks the entity as updated
    /// </summary>
    protected void MarkAsUpdated(string? updatedBy = null)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }

    public bool Equals(Entity<TId>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Entity<TId>)obj);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id!);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}