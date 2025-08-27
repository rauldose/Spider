using Spider.Core.SharedKernel.Abstractions;
using Spider.ProjectManagement.Infrastructure.Data;

namespace Spider.ProjectManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ProjectManagementDbContext _context;

    public UnitOfWork(ProjectManagementDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction != null)
        {
            await _context.Database.CommitTransactionAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction != null)
        {
            await _context.Database.RollbackTransactionAsync(cancellationToken);
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync(cancellationToken);
        try
        {
            await action();
            await CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> func, CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await func();
            await CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}