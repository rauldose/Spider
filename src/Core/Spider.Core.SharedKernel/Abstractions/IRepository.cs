using System.Linq.Expressions;
using Spider.Core.SharedKernel.Base;

namespace Spider.Core.SharedKernel.Abstractions;

/// <summary>
/// Generic repository interface following repository pattern
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TId">The entity ID type</typeparam>
public interface IRepository<TEntity, TId> where TEntity : Entity<TId>
{
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds entities matching the specification
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a single entity matching the predicate
    /// </summary>
    Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new entity
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes an entity
    /// </summary>
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes an entity by its ID
    /// </summary>
    Task RemoveAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Counts entities matching the specification
    /// </summary>
    Task<int> CountAsync(ISpecification<TEntity>? specification = null, CancellationToken cancellationToken = default);
}