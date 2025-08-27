using Microsoft.EntityFrameworkCore;
using Spider.ConnectionManagement.Application.Interfaces;
using Spider.ConnectionManagement.Domain.Entities;
using Spider.ConnectionManagement.Infrastructure.Persistence;
using Spider.Core.SharedKernel.Abstractions;
using System.Linq.Expressions;

namespace Spider.ConnectionManagement.Infrastructure.Repositories;

public class ConnectionRepository : IConnectionRepository
{
    private readonly ConnectionManagementDbContext _context;

    public ConnectionRepository(ConnectionManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Connection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Connections
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Connection>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Connections
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Connection>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        return await _context.Connections
            .Where(c => c.DeviceId == deviceId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Connection>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Connections
            .Where(c => c.Status.Name == status)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Connection>> GetUnhealthyAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Connections
            .Where(c => !c.Health.IsHealthy)
            .OrderByDescending(c => c.Health.ConsecutiveFailures)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByDeviceIdAndNameAsync(Guid deviceId, string name, CancellationToken cancellationToken = default)
    {
        return await _context.Connections
            .AnyAsync(c => c.DeviceId == deviceId && c.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Connection>> FindAsync(ISpecification<Connection> specification, CancellationToken cancellationToken = default)
    {
        var query = _context.Connections.AsQueryable();

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        foreach (var includeString in specification.IncludeStrings)
        {
            query = query.Include(includeString);
        }

        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

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

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Connection?> FindSingleAsync(Expression<Func<Connection, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Connections
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<Connection> AddAsync(Connection entity, CancellationToken cancellationToken = default)
    {
        await _context.Connections.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<Connection> UpdateAsync(Connection entity, CancellationToken cancellationToken = default)
    {
        _context.Connections.Update(entity);
        return await Task.FromResult(entity);
    }

    public async Task RemoveAsync(Connection entity, CancellationToken cancellationToken = default)
    {
        _context.Connections.Remove(entity);
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

    public async Task<int> CountAsync(ISpecification<Connection>? specification = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Connections.AsQueryable();

        if (specification?.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Connections
            .AnyAsync(c => c.Id == id, cancellationToken);
    }
}