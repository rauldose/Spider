using Spider.Communication.Domain.Entities;
using Spider.Core.SharedKernel.Abstractions;
using Spider.Core.Application.Interfaces;
using Spider.Core.Application.Common;

namespace Spider.Communication.Application.Interfaces;

/// <summary>
/// Repository interface for Link entities
/// </summary>
public interface ILinkRepository : IRepository<Link, Guid>
{
    Task<Link?> GetByIdWithChannelsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Link?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IPagedResult<Link>> GetByStatusPagedAsync(string status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IPagedResult<Link>> GetByProtocolPagedAsync(string protocolType, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Link>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<List<Link>> GetConnectedLinksAsync(CancellationToken cancellationToken = default);
    Task<List<Link>> GetHealthyLinksAsync(CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<int> CountHealthyAsync(CancellationToken cancellationToken = default);
    Task<TimeSpan> GetAverageResponseTimeAsync(CancellationToken cancellationToken = default);
    Task<IPagedResult<Link>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Channel entities
/// </summary>
public interface IChannelRepository : IRepository<Channel, Guid>
{
    Task<Channel?> GetByIdWithDataPointsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Channel>> GetByLinkIdAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<List<Channel>> GetActiveChannelsAsync(CancellationToken cancellationToken = default);
    Task<List<Channel>> GetByTypeAsync(string channelType, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<IPagedResult<Channel>> GetByLinkIdPagedAsync(Guid linkId, int page, int pageSize, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for DataPoint entities
/// </summary>
public interface IDataPointRepository : IRepository<DataPoint, Guid>
{
    Task<List<DataPoint>> GetByChannelIdAsync(Guid channelId, CancellationToken cancellationToken = default);
    Task<List<DataPoint>> GetByLinkIdAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<List<DataPoint>> GetActiveDataPointsAsync(CancellationToken cancellationToken = default);
    Task<List<DataPoint>> GetByAddressAsync(string address, CancellationToken cancellationToken = default);
    Task<List<DataPoint>> GetByDataTypeAsync(string dataType, CancellationToken cancellationToken = default);
    Task<List<DataPoint>> GetByAccessModeAsync(string accessMode, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<int> CountByChannelIdAsync(Guid channelId, CancellationToken cancellationToken = default);
    Task<Channel?> GetChannelByDataPointIdAsync(Guid dataPointId, CancellationToken cancellationToken = default);
    Task<IPagedResult<DataPoint>> GetByChannelIdPagedAsync(Guid channelId, int page, int pageSize, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for communication management operations
/// </summary>
public interface ICommunicationService
{
    Task<bool> StartLinkAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<bool> StopLinkAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<bool> RestartLinkAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> DiagnoseLinkAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task StartRealTimeDataCollectionAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task StopRealTimeDataCollectionAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<object?> ReadDataPointValueAsync(Guid dataPointId, CancellationToken cancellationToken = default);
    Task<bool> WriteDataPointValueAsync(Guid dataPointId, object value, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, object>> BulkReadDataPointsAsync(List<Guid> dataPointIds, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for real-time data operations
/// </summary>
public interface IRealTimeDataService
{
    Task<Dictionary<string, object>> GetCurrentDataAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task SubscribeToDataChangesAsync(Guid linkId, Func<Dictionary<string, object>, Task> onDataChanged, CancellationToken cancellationToken = default);
    Task UnsubscribeFromDataChangesAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<bool> IsSubscribedAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for health monitoring operations
/// </summary>
public interface IHealthMonitoringService
{
    Task<bool> IsLinkHealthyAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetHealthMetricsAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetUnhealthyLinksAsync(CancellationToken cancellationToken = default);
    Task StartHealthMonitoringAsync(Guid linkId, TimeSpan interval, CancellationToken cancellationToken = default);
    Task StopHealthMonitoringAsync(Guid linkId, CancellationToken cancellationToken = default);
    Task<bool> IsMonitoringAsync(Guid linkId, CancellationToken cancellationToken = default);
}