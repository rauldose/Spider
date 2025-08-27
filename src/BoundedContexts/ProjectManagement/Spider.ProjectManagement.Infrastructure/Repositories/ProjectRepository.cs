using Microsoft.EntityFrameworkCore;
using Spider.Core.SharedKernel.Abstractions;
using Spider.ProjectManagement.Domain.Entities;
using Spider.ProjectManagement.Infrastructure.Data;
using System.Linq.Expressions;

namespace Spider.ProjectManagement.Infrastructure.Repositories;

public class ProjectRepository : IRepository<Project, Guid>
{
    private readonly ProjectManagementDbContext _context;
    private readonly DbSet<Project> _dbSet;

    public ProjectRepository(ProjectManagementDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Project>();
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ParentProject)
            .Include(p => p.ChildProjects)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ParentProject)
            .Include(p => p.ChildProjects)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> FindAsync(ISpecification<Project> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<Project?> FindSingleAsync(Expression<Func<Project, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ParentProject)
            .Include(p => p.ChildProjects)
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<Project> AddAsync(Project entity, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public Task<Project> UpdateAsync(Project entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public async Task RemoveAsync(Project entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
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
        return await _dbSet.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<Project>? specification = null, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    private IQueryable<Project> ApplySpecification(ISpecification<Project> specification)
    {
        var query = _dbSet
            .Include(p => p.ParentProject)
            .Include(p => p.ChildProjects)
            .AsQueryable();

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