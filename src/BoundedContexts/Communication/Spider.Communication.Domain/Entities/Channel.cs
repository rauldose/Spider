using Spider.Core.SharedKernel.Base;
using Spider.Core.SharedKernel.Abstractions;
using Spider.Communication.Domain.ValueObjects;
using Spider.Communication.Domain.Events;
using Spider.Drivers.Core.Abstractions;
using Spider.Drivers.Core.Models;

namespace Spider.Communication.Domain.Entities;

/// <summary>
/// Channel entity representing a communication channel for data exchange
/// Following DDD patterns with proper business logic encapsulation
/// </summary>
public class Channel : AggregateRoot<Guid>
{
    private readonly List<DataPoint> _dataPoints = new();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public ChannelType Type { get; private set; }
    public ChannelConfiguration Configuration { get; private set; }
    public ChannelStatus Status { get; private set; }
    public ChannelHealth Health { get; private set; }
    public Guid? LinkId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActivity { get; private set; }
    public IReadOnlyList<DataPoint> DataPoints => _dataPoints.AsReadOnly();

    // Private constructor for EF Core
    private Channel() { }

    /// <summary>
    /// Create a new Channel
    /// </summary>
    public Channel(string name, string description, ChannelType type, ChannelConfiguration configuration) 
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Channel name cannot be empty", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Status = ChannelStatus.Inactive;
        Health = ChannelHealth.Unknown();
        CreatedAt = DateTime.UtcNow;
        LastActivity = CreatedAt;

        AddDomainEvent(new ChannelCreatedEvent(Id, name, type.Name));
    }

    /// <summary>
    /// Assign this channel to a link
    /// </summary>
    internal void AssignToLink(Guid linkId)
    {
        if (LinkId.HasValue)
            throw new InvalidOperationException($"Channel is already assigned to link {LinkId}");

        LinkId = linkId;
        AddDomainEvent(new ChannelAssignedToLinkEvent(Id, Name, linkId));
    }

    /// <summary>
    /// Unassign this channel from its current link
    /// </summary>
    internal void UnassignFromLink()
    {
        if (!LinkId.HasValue)
            return;

        var previousLinkId = LinkId.Value;
        LinkId = null;
        
        AddDomainEvent(new ChannelUnassignedFromLinkEvent(Id, Name, previousLinkId));
    }

    /// <summary>
    /// Activate the channel for communication
    /// </summary>
    public void Activate()
    {
        if (Status == ChannelStatus.Active)
            return;

        if (!LinkId.HasValue)
            throw new InvalidOperationException("Channel must be assigned to a link before activation");

        ChangeStatus(ChannelStatus.Active, "Channel activated");
        Health = ChannelHealth.Good(DateTime.UtcNow);
        UpdateLastActivity();
    }

    /// <summary>
    /// Deactivate the channel
    /// </summary>
    public void Deactivate()
    {
        if (Status == ChannelStatus.Inactive)
            return;

        ChangeStatus(ChannelStatus.Inactive, "Channel deactivated");
        Health = ChannelHealth.Unknown();
        UpdateLastActivity();
    }

    /// <summary>
    /// Add a data point to this channel
    /// </summary>
    public void AddDataPoint(DataPoint dataPoint)
    {
        if (dataPoint == null)
            throw new ArgumentNullException(nameof(dataPoint));

        if (_dataPoints.Any(dp => dp.Address == dataPoint.Address))
            throw new InvalidOperationException($"Data point with address '{dataPoint.Address}' already exists");

        if (_dataPoints.Count >= Configuration.MaxDataPoints)
            throw new InvalidOperationException($"Maximum number of data points ({Configuration.MaxDataPoints}) reached");

        _dataPoints.Add(dataPoint);
        dataPoint.AssignToChannel(Id);
        
        AddDomainEvent(new ChannelDataPointAddedEvent(Id, dataPoint.Id, dataPoint.Address));
    }

    /// <summary>
    /// Remove a data point from this channel
    /// </summary>
    public void RemoveDataPoint(Guid dataPointId)
    {
        var dataPoint = _dataPoints.FirstOrDefault(dp => dp.Id == dataPointId);
        if (dataPoint == null)
            throw new InvalidOperationException($"Data point with ID '{dataPointId}' not found");

        _dataPoints.Remove(dataPoint);
        dataPoint.UnassignFromChannel();
        
        AddDomainEvent(new ChannelDataPointRemovedEvent(Id, dataPointId, dataPoint.Address));
    }

    /// <summary>
    /// Read data from a specific data point using the assigned link's driver
    /// </summary>
    public async Task<ChannelReadResult> ReadDataPointAsync(string address, IDriver driver, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty", nameof(address));

        if (driver == null)
            throw new ArgumentNullException(nameof(driver));

        if (Status != ChannelStatus.Active)
            return ChannelReadResult.CreateFailure($"Channel is not active (Status: {Status.Name})");

        var dataPoint = _dataPoints.FirstOrDefault(dp => dp.Address == address);
        if (dataPoint == null)
            return ChannelReadResult.CreateFailure($"Data point with address '{address}' not found");

        try
        {
            if (driver is IReadableDriver readableDriver)
            {
                var request = new ReadRequest(address, dataPoint.DataType.Name, dataPoint.Length);
                var result = await readableDriver.ReadAsync(request, cancellationToken);
                
                if (result.Success)
                {
                    // Update data point with new value
                    dataPoint.UpdateValue(result.Value, result.DataQuality, result.Timestamp);
                    Health = ChannelHealth.Good(DateTime.UtcNow);
                    UpdateLastActivity();
                    
                    AddDomainEvent(new ChannelDataReadEvent(Id, address, result.Value, result.Timestamp));
                    return ChannelReadResult.CreateSuccess(result.Value, result.DataQuality, result.Timestamp);
                }
                else
                {
                    Health = ChannelHealth.Bad(result.ErrorMessage ?? "Read failed", DateTime.UtcNow);
                    AddDomainEvent(new ChannelErrorOccurredEvent(Id, "READ_ERROR", result.ErrorMessage ?? "Read failed"));
                    return ChannelReadResult.CreateFailure(result.ErrorMessage ?? "Read operation failed");
                }
            }
            else
            {
                return ChannelReadResult.CreateFailure("Driver does not support reading operations");
            }
        }
        catch (Exception ex)
        {
            Health = ChannelHealth.Bad(ex.Message, DateTime.UtcNow);
            AddDomainEvent(new ChannelErrorOccurredEvent(Id, "READ_EXCEPTION", ex.Message));
            return ChannelReadResult.CreateFailure($"Read operation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Write data to a specific data point using the assigned link's driver
    /// </summary>
    public async Task<ChannelWriteResult> WriteDataPointAsync(string address, object value, IDriver driver, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty", nameof(address));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (driver == null)
            throw new ArgumentNullException(nameof(driver));

        if (Status != ChannelStatus.Active)
            return ChannelWriteResult.CreateFailure($"Channel is not active (Status: {Status.Name})");

        var dataPoint = _dataPoints.FirstOrDefault(dp => dp.Address == address);
        if (dataPoint == null)
            return ChannelWriteResult.CreateFailure($"Data point with address '{address}' not found");

        if (!dataPoint.IsWritable)
            return ChannelWriteResult.CreateFailure($"Data point '{address}' is not writable");

        try
        {
            if (driver is IWritableDriver writableDriver)
            {
                var request = new WriteRequest(address, value, dataPoint.DataType.Name);
                var result = await writableDriver.WriteAsync(request, cancellationToken);
                
                if (result.Success)
                {
                    // Update data point with new value
                    dataPoint.UpdateValue(value, "Good", result.Timestamp);
                    Health = ChannelHealth.Good(DateTime.UtcNow);
                    UpdateLastActivity();
                    
                    AddDomainEvent(new ChannelDataWrittenEvent(Id, address, value, result.Timestamp));
                    return ChannelWriteResult.CreateSuccess(result.Timestamp);
                }
                else
                {
                    Health = ChannelHealth.Bad(result.ErrorMessage ?? "Write failed", DateTime.UtcNow);
                    AddDomainEvent(new ChannelErrorOccurredEvent(Id, "WRITE_ERROR", result.ErrorMessage ?? "Write failed"));
                    return ChannelWriteResult.CreateFailure(result.ErrorMessage ?? "Write operation failed");
                }
            }
            else
            {
                return ChannelWriteResult.CreateFailure("Driver does not support writing operations");
            }
        }
        catch (Exception ex)
        {
            Health = ChannelHealth.Bad(ex.Message, DateTime.UtcNow);
            AddDomainEvent(new ChannelErrorOccurredEvent(Id, "WRITE_EXCEPTION", ex.Message));
            return ChannelWriteResult.CreateFailure($"Write operation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Perform health check on the channel
    /// </summary>
    public async Task<ChannelHealthResult> PerformHealthCheckAsync(CancellationToken cancellationToken = default)
    {
        if (Status != ChannelStatus.Active)
        {
            Health = ChannelHealth.Unknown();
            return ChannelHealthResult.CreateUnhealthy($"Channel not active (Status: {Status.Name})");
        }

        try
        {
            // Check if all data points are accessible
            var healthyDataPoints = 0;
            var totalDataPoints = _dataPoints.Count;

            foreach (var dataPoint in _dataPoints)
            {
                if (dataPoint.LastUpdated.HasValue && 
                    (DateTime.UtcNow - dataPoint.LastUpdated.Value) < Configuration.HealthCheckInterval)
                {
                    healthyDataPoints++;
                }
            }

            var healthPercentage = totalDataPoints > 0 ? (double)healthyDataPoints / totalDataPoints * 100 : 100;

            if (healthPercentage >= 80)
            {
                Health = ChannelHealth.Good(DateTime.UtcNow);
                return ChannelHealthResult.CreateHealthy($"Health: {healthPercentage:F1}% ({healthyDataPoints}/{totalDataPoints} data points)");
            }
            else
            {
                Health = ChannelHealth.Bad($"Low health percentage: {healthPercentage:F1}%", DateTime.UtcNow);
                return ChannelHealthResult.CreateUnhealthy($"Low health: {healthPercentage:F1}% ({healthyDataPoints}/{totalDataPoints} data points)");
            }
        }
        catch (Exception ex)
        {
            Health = ChannelHealth.Bad(ex.Message, DateTime.UtcNow);
            AddDomainEvent(new ChannelErrorOccurredEvent(Id, "HEALTH_CHECK_ERROR", ex.Message));
            return ChannelHealthResult.CreateUnhealthy($"Health check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Update channel configuration
    /// </summary>
    public void UpdateConfiguration(ChannelConfiguration newConfiguration)
    {
        if (newConfiguration == null)
            throw new ArgumentNullException(nameof(newConfiguration));

        if (Status == ChannelStatus.Active)
            throw new InvalidOperationException("Cannot update configuration while channel is active");

        var previousConfig = Configuration;
        Configuration = newConfiguration;
        
        AddDomainEvent(new ChannelConfigurationUpdatedEvent(Id, Name, previousConfig, newConfiguration));
    }

    /// <summary>
    /// Update channel name and description
    /// </summary>
    public void UpdateNameAndDescription(string newName, string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Channel name cannot be empty", nameof(newName));

        var previousName = Name;
        var previousDescription = Description;
        
        Name = newName;
        Description = newDescription ?? string.Empty;
        
        AddDomainEvent(new ChannelRenamedEvent(Id, previousName, newName, previousDescription, Description));
    }

    private void ChangeStatus(ChannelStatus newStatus, string? reason = null)
    {
        if (Status != newStatus)
        {
            var previousStatus = Status;
            Status = newStatus;
            
            AddDomainEvent(new ChannelStatusChangedEvent(Id, Name, previousStatus.Name, newStatus.Name, reason));
        }
    }

    private void UpdateLastActivity()
    {
        LastActivity = DateTime.UtcNow;
    }
}