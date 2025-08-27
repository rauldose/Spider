namespace Spider.Core.SharedKernel.Abstractions;

/// <summary>
/// Unit of Work pattern for managing transactions across multiple repositories
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes to the underlying data store
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begins a new transaction
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes the action within a transaction scope
    /// </summary>
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes the function within a transaction scope and returns the result
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> func, CancellationToken cancellationToken = default);
}