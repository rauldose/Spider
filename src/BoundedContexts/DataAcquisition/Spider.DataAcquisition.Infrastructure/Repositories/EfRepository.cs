using Microsoft.EntityFrameworkCore;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DataAcquisition.Infrastructure.Data;
using System.Linq.Expressions;

namespace Spider.DataAcquisition.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation using Entity Framework
/// </summary>
public class EfRepository<TEntity, TId> : IRepository<TEntity, TId> 
    where TEntity : Spider.Core.SharedKernel.Base.Entity<TId>
{
    private readonly DataAcquisitionDbContext _context;

    public EfRepository(DataAcquisitionDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().FindAsync(new object[] { id! }, cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> FindAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var result = _context.Set<TEntity>().Add(entity);
        return result.Entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public async Task RemoveAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _context.Set<TEntity>().Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().AnyAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<TEntity>? specification = null, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            return await _context.Set<TEntity>().CountAsync(cancellationToken);
        }

        var query = ApplySpecification(specification);
        return await query.CountAsync(cancellationToken);
    }

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
    {
        var query = _context.Set<TEntity>().AsQueryable();

        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        if (spec.GroupBy != null)
        {
            query = query.GroupBy(spec.GroupBy).SelectMany(x => x);
        }

        if (spec.IsPagingEnabled)
        {
            if (spec.Skip.HasValue)
            {
                query = query.Skip(spec.Skip.Value);
            }
            if (spec.Take.HasValue)
            {
                query = query.Take(spec.Take.Value);
            }
        }

        return query;
    }
}