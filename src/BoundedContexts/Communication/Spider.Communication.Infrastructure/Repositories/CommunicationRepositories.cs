using Microsoft.EntityFrameworkCore;
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

    public LinkRepository(CommunicationDbContext context)
    {
        _context = context;
    }

    public async Task<Link?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Links
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

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

    public async Task<List<Link>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Links
            .Include(l => l.Channels)
            .ToListAsync(cancellationToken);
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

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Links.CountAsync(cancellationToken);
    }

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
        var averageMs = await _context.Links
            .Where(l => l.Health.IsHealthy)
            .AverageAsync(l => l.Health.AverageResponseTime.TotalMilliseconds, cancellationToken);

        return TimeSpan.FromMilliseconds(averageMs);
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

    public async Task AddAsync(Link entity, CancellationToken cancellationToken = default)
    {
        await _context.Links.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Link entity, CancellationToken cancellationToken = default)
    {
        _context.Links.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Link entity, CancellationToken cancellationToken = default)
    {
        _context.Links.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Links.AnyAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<List<Link>> FindAsync(ISpecification<Link> specification, CancellationToken cancellationToken = default)
    {
        return await _context.Links
            .Where(specification.ToExpression())
            .Include(l => l.Channels)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// EF Core implementation of IChannelRepository
/// </summary>
public class ChannelRepository : IChannelRepository
{
    private readonly CommunicationDbContext _context;

    public ChannelRepository(CommunicationDbContext context)
    {
        _context = context;
    }

    public async Task<Channel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

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

    public async Task<List<Channel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Channels
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
            .Where(c => c.IsEnabled)
            .Include(c => c.DataPoints)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Channel>> GetByTypeAsync(string channelType, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .Where(c => c.ChannelType.ToString() == channelType)
            .Include(c => c.DataPoints)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Channels.CountAsync(cancellationToken);
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .CountAsync(c => c.IsEnabled, cancellationToken);
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

    public async Task AddAsync(Channel entity, CancellationToken cancellationToken = default)
    {
        await _context.Channels.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Channel entity, CancellationToken cancellationToken = default)
    {
        _context.Channels.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Channel entity, CancellationToken cancellationToken = default)
    {
        _context.Channels.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Channels.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Channel>> FindAsync(ISpecification<Channel> specification, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .Where(specification.ToExpression())
            .Include(c => c.DataPoints)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// EF Core implementation of IDataPointRepository
/// </summary>
public class DataPointRepository : IDataPointRepository
{
    private readonly CommunicationDbContext _context;

    public DataPointRepository(CommunicationDbContext context)
    {
        _context = context;
    }

    public async Task<DataPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .FirstOrDefaultAsync(dp => dp.Id == id, cancellationToken);
    }

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

    public async Task<List<DataPoint>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints.ToListAsync(cancellationToken);
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
        return await _context.DataPoints
            .Where(dp => dp.IsEnabled)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DataPoint>> GetByAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .Where(dp => dp.Address.Value == address)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DataPoint>> GetByDataTypeAsync(string dataType, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .Where(dp => dp.DataType.ToString() == dataType)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DataPoint>> GetByAccessModeAsync(string accessMode, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .Where(dp => dp.AccessMode.ToString() == accessMode)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints.CountAsync(cancellationToken);
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .CountAsync(dp => dp.IsEnabled, cancellationToken);
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

    public async Task AddAsync(DataPoint entity, CancellationToken cancellationToken = default)
    {
        await _context.DataPoints.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(DataPoint entity, CancellationToken cancellationToken = default)
    {
        _context.DataPoints.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(DataPoint entity, CancellationToken cancellationToken = default)
    {
        _context.DataPoints.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints.AnyAsync(dp => dp.Id == id, cancellationToken);
    }

    public async Task<List<DataPoint>> FindAsync(ISpecification<DataPoint> specification, CancellationToken cancellationToken = default)
    {
        return await _context.DataPoints
            .Where(specification.ToExpression())
            .ToListAsync(cancellationToken);
    }
}