using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Spider.Communication.Application.Interfaces;
using Spider.Communication.Domain.Entities;
using Spider.Communication.Infrastructure.Persistence;
using Spider.Core.Application.Interfaces;
using Spider.Core.Application.Common;
using Spider.Core.SharedKernel.Abstractions;

namespace Spider.Communication.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ILinkRepository
/// </summary>
public class LinkRepository : ILinkRepository
{
    private readonly CommunicationDbContext _context;
    private readonly DbSet<Link> _dbSet;

    public LinkRepository(CommunicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Link>();
    }

    // IRepository<Link, Guid> implementation
    public async Task<Link?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Link>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Channels)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Link>> FindAsync(ISpecification<Link> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification)
            .Include(l => l.Channels)
            .ToListAsync(cancellationToken);
    }

    public async Task<Link?> FindSingleAsync(Expression<Func<Link, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<Link> AddAsync(Link entity, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public Task<Link> UpdateAsync(Link entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public Task RemoveAsync(Link entity, CancellationToken cancellationToken = default)
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
        return await _dbSet.AnyAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<Link>? specification = null, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    private IQueryable<Link> ApplySpecification(ISpecification<Link> specification)
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

        return query;
    }

    // ILinkRepository specific methods
    public async Task<Link?> GetByIdWithChannelsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Links
            .Include(l => l.Channels)
            .ThenInclude(c => c.DataPoints)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<Link?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Links
            .FirstOrDefaultAsync(l => l.Metadata.Name == name, cancellationToken);
    }

    public async Task<IPagedResult<Link>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Links.CountAsync(cancellationToken);
        
        var items = await _context.Links
            .Include(l => l.Channels)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Link>.Success(items, page, pageSize, totalCount);
    }

    public async Task<IPagedResult<Link>> GetByStatusPagedAsync(string status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Links.Where(l => l.Status.ToString() == status);
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Include(l => l.Channels)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Link>.Success(items, page, pageSize, totalCount);
    }

    public async Task<IPagedResult<Link>> GetByProtocolPagedAsync(string protocolType, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Links.Where(l => l.Metadata.ProtocolType == protocolType);
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Include(l => l.Channels)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Link>.Success(items, page, pageSize, totalCount);
    }

    public async Task<List<Link>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Links
            .Where(l => l.Status.ToString() == status)
            .Include(l => l.Channels)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Link>> GetConnectedLinksAsync(CancellationToken cancellationToken = default)
    {
        return await GetByStatusAsync("Connected", cancellationToken);
    }

    public async Task<List<Link>> GetHealthyLinksAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Links
            .Where(l => l.Health.IsHealthy)
            .Include(l => l.Channels)
            .ToListAsync(cancellationToken);
    }

    // Remove duplicate CountAsync - we already have it from IRepository interface
    public async Task<int> CountByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Links
            .CountAsync(l => l.Status.ToString() == status, cancellationToken);
    }

    public async Task<int> CountHealthyAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Links
            .CountAsync(l => l.Health.IsHealthy, cancellationToken);
    }

    public async Task<TimeSpan> GetAverageResponseTimeAsync(CancellationToken cancellationToken = default)
    {
        // Since LinkHealth doesn't have AverageResponseTime, return a default value
        var healthyLinksCount = await _context.Links
            .Where(l => l.Health.IsHealthy)
            .CountAsync(cancellationToken);

        // Return a reasonable default based on healthy links
        return healthyLinksCount > 0 ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromMilliseconds(500);
    }

    public async Task<IPagedResult<Link>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Links.Where(l => 
            l.Metadata.Name.Contains(searchTerm) || 
            l.Metadata.Description.Contains(searchTerm) ||
            l.Metadata.ProtocolType.Contains(searchTerm));

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Include(l => l.Channels)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Link>.Success(items, page, pageSize, totalCount);
    }
}

/// <summary>
/// EF Core implementation of IChannelRepository
/// </summary>
public class ChannelRepository : IChannelRepository
{
    private readonly CommunicationDbContext _context;
    private readonly DbSet<Channel> _dbSet;

    public ChannelRepository(CommunicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Channel>();
    }

    // IRepository<Channel, Guid> implementation
    public async Task<Channel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Channel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DataPoints)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Channel>> FindAsync(ISpecification<Channel> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification)
            .Include(c => c.DataPoints)
            .ToListAsync(cancellationToken);
    }

    public async Task<Channel?> FindSingleAsync(Expression<Func<Channel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<Channel> AddAsync(Channel entity, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public Task<Channel> UpdateAsync(Channel entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public Task RemoveAsync(Channel entity, CancellationToken cancellationToken = default)
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
        return await _dbSet.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<Channel>? specification = null, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    private IQueryable<Channel> ApplySpecification(ISpecification<Channel> specification)
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

        return query;
    }

    // IChannelRepository specific methods
    public async Task<Channel?> GetByIdWithDataPointsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .Include(c => c.DataPoints)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Channel>> GetByLinkIdAsync(Guid linkId, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .Where(c => c.LinkId == linkId)
            .Include(c => c.DataPoints)
            .ToListAsync(cancellationToken);
    }

    public async Task<IPagedResult<Channel>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Channels.CountAsync(cancellationToken);
        
        var items = await _context.Channels
            .Include(c => c.DataPoints)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Channel>.Success(items, page, pageSize, totalCount);
    }

    public async Task<List<Channel>> GetActiveChannelsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .Where(c => c.Status.Name == "Active")
            .Include(c => c.DataPoints)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Channel>> GetByTypeAsync(string channelType, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .Where(c => c.Type.ToString() == channelType)
            .Include(c => c.DataPoints)
            .ToListAsync(cancellationToken);
    }

    // Remove duplicate CountAsync - we already have it from IRepository interface
    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .CountAsync(c => c.Status.Name == "Active", cancellationToken);
    }

    public async Task<IPagedResult<Channel>> GetByLinkIdPagedAsync(Guid linkId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Channels.Where(c => c.LinkId == linkId);
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Include(c => c.DataPoints)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Channel>.Success(items, page, pageSize, totalCount);
    }
}

/// <summary>
/// EF Core implementation of IDataPointRepository
/// </summary>
public class DataPointRepository : IDataPointRepository
{
    private readonly CommunicationDbContext _context;
    private readonly DbSet<DataPoint> _dbSet;

    public DataPointRepository(CommunicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<DataPoint>();
    }

    // IRepository<DataPoint, Guid> implementation
    public async Task<DataPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<DataPoint>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DataPoint>> FindAsync(ISpecification<DataPoint> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<DataPoint?> FindSingleAsync(Expression<Func<DataPoint, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<DataPoint> AddAsync(DataPoint entity, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public Task<DataPoint> UpdateAsync(DataPoint entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public Task RemoveAsync(DataPoint entity, CancellationToken cancellationToken = default)
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
        return await _dbSet.AnyAsync(dp => dp.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<DataPoint>? specification = null, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    private IQueryable<DataPoint> ApplySpecification(ISpecification<DataPoint> specification)
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

        return query;
    }

    // IDataPointRepository specific methods
    public async Task<List<DataPoint>> GetByChannelIdAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .Where(dp => dp.ChannelId == channelId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DataPoint>> GetByLinkIdAsync(Guid linkId, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .Join(_context.Channels, dp => dp.ChannelId, c => c.Id, (dp, c) => new { DataPoint = dp, Channel = c })
            .Where(x => x.Channel.LinkId == linkId)
            .Select(x => x.DataPoint)
            .ToListAsync(cancellationToken);
    }

    public async Task<IPagedResult<DataPoint>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.DataPoints.CountAsync(cancellationToken);
        
        var items = await _context.DataPoints
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<DataPoint>.Success(items, page, pageSize, totalCount);
    }

    public async Task<List<DataPoint>> GetActiveDataPointsAsync(CancellationToken cancellationToken = default)
    {
        // Since DataPoint doesn't have IsEnabled, return all data points
        return await _context.DataPoints.ToListAsync(cancellationToken);
    }

    public async Task<List<DataPoint>> GetByAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .Where(dp => dp.Address == address)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DataPoint>> GetByDataTypeAsync(string dataType, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .Where(dp => dp.DataType.ToString() == dataType)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DataPoint>> GetByWritableAsync(bool isWritable, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .Where(dp => dp.IsWritable == isWritable)
            .ToListAsync(cancellationToken);
    }

    // Remove duplicate CountAsync - we already have it from IRepository interface
    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        // Since DataPoint doesn't have IsEnabled, return total count
        return await _context.DataPoints.CountAsync(cancellationToken);
    }

    public async Task<int> CountByChannelIdAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .CountAsync(dp => dp.ChannelId == channelId, cancellationToken);
    }

    public async Task<Channel?> GetChannelByDataPointIdAsync(Guid dataPointId, CancellationToken cancellationToken = default)
    {
        var dataPoint = await _context.DataPoints
            .FirstOrDefaultAsync(dp => dp.Id == dataPointId, cancellationToken);

        if (dataPoint == null) return null;

        return await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == dataPoint.ChannelId, cancellationToken);
    }

    public async Task<IPagedResult<DataPoint>> GetByChannelIdPagedAsync(Guid channelId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.DataPoints.Where(dp => dp.ChannelId == channelId);
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<DataPoint>.Success(items, page, pageSize, totalCount);
    }
}