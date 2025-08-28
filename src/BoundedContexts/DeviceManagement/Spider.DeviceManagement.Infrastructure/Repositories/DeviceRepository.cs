using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Spider.Core.SharedKernel.Abstractions;
using Spider.Core.SharedKernel.Base;
using Spider.DeviceManagement.Domain.Entities;
using Spider.DeviceManagement.Infrastructure.Persistence;

namespace Spider.DeviceManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Device entities
/// </summary>
public class DeviceRepository : IRepository<Device, Guid>
{
    private readonly DeviceManagementDbContext _context;
    private readonly DbSet<Device> _dbSet;

    public DeviceRepository(DeviceManagementDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Device>();
    }

    public async Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Device>> FindAsync(ISpecification<Device> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<Device?> FindSingleAsync(Expression<Func<Device, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<Device> AddAsync(Device entity, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public Task<Device> UpdateAsync(Device entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public Task RemoveAsync(Device entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await RemoveAsync(entity, cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<Device>? specification = null, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    private IQueryable<Device> ApplySpecification(ISpecification<Device> specification)
    {
        var query = _dbSet.AsQueryable();

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes
        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply grouping
        if (specification.GroupBy != null)
        {
            query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            if (specification.Skip.HasValue)
            {
                query = query.Skip(specification.Skip.Value);
            }

            if (specification.Take.HasValue)
            {
                query = query.Take(specification.Take.Value);
            }
        }

        return query;
    }
}