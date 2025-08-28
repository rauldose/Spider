using Spider.Core.SharedKernel.Base;
using Spider.Core.SharedKernel.Abstractions;
using Spider.Communication.Domain.ValueObjects;
using Spider.Communication.Domain.Events;
using Spider.Drivers.Core.Abstractions;
using Spider.Drivers.Core.Models;

namespace Spider.Communication.Domain.Entities;

/// <summary>
/// Link entity representing a communication link between devices and channels
/// Following DDD patterns with proper encapsulation and business logic
/// </summary>
public class Link : AggregateRoot<Guid>
{
    private readonly List<Channel> _channels = new();
    
    public LinkMetadata Metadata { get; private set; }
    public LinkConfiguration Configuration { get; private set; }
    public LinkStatus Status { get; private set; }
    public LinkHealth Health { get; private set; }
    public IDriver? Driver { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActivity { get; private set; }
    public IReadOnlyList<Channel> Channels => _channels.AsReadOnly();

    // Private constructor for EF Core
    private Link() { }

    /// <summary>
    /// Create a new Link with specified metadata and configuration
    /// </summary>
    public Link(LinkMetadata metadata, LinkConfiguration configuration) : base(Guid.NewGuid())
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Status = LinkStatus.Disconnected;
        Health = LinkHealth.Unknown();
        CreatedAt = DateTime.UtcNow;
        LastActivity = CreatedAt;

        AddDomainEvent(new LinkCreatedEvent(Id, metadata.Name, metadata.ProtocolType));
    }

    /// <summary>
    /// Attach a driver to this link
    /// </summary>
    public void AttachDriver(IDriver driver)
    {
        if (driver == null)
            throw new ArgumentNullException(nameof(driver));

        if (Status == LinkStatus.Connected)
            throw new InvalidOperationException("Cannot attach driver to connected link");

        var previousDriver = Driver;
        Driver = driver;
        
        // Subscribe to driver events
        driver.StatusChanged += OnDriverStatusChanged;
        driver.ErrorOccurred += OnDriverErrorOccurred;
        
        AddDomainEvent(new LinkDriverAttachedEvent(Id, driver.Metadata.Name, previousDriver?.Metadata.Name));
    }

    /// <summary>
    /// Detach the current driver from this link
    /// </summary>
    public void DetachDriver()
    {
        if (Driver == null) return;

        if (Status == LinkStatus.Connected)
            throw new InvalidOperationException("Cannot detach driver from connected link");

        var driverName = Driver.Metadata.Name;
        
        // Unsubscribe from driver events
        Driver.StatusChanged -= OnDriverStatusChanged;
        Driver.ErrorOccurred -= OnDriverErrorOccurred;
        
        Driver = null;
        
        AddDomainEvent(new LinkDriverDetachedEvent(Id, driverName));
    }

    /// <summary>
    /// Connect the link using the attached driver
    /// </summary>
    public async Task<LinkOperationResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (Driver == null)
            return LinkOperationResult.CreateFailure("No driver attached to link");

        if (Status == LinkStatus.Connected)
            return LinkOperationResult.CreateSuccess("Link already connected");

        try
        {
            ChangeStatus(LinkStatus.Connecting, "Connection initiated");
            
            // Initialize driver if needed
            if (Driver.Status == DriverStatus.Uninitialized)
            {
                var driverConfig = new DriverConfiguration(
                    Configuration.ConnectionString,
                    Configuration.Parameters.ToDictionary(p => p.Key, p => p.Value),
                    Configuration.ConnectionTimeout,
                    Configuration.OperationTimeout);
                
                var initResult = await Driver.InitializeAsync(driverConfig, cancellationToken);
                if (!initResult.Success)
                {
                    ChangeStatus(LinkStatus.Error, $"Driver initialization failed: {initResult.ErrorMessage}");
                    return LinkOperationResult.CreateFailure($"Driver initialization failed: {initResult.ErrorMessage}");
                }
            }

            // Update health and status
            Health = LinkHealth.Good(DateTime.UtcNow);
            ChangeStatus(LinkStatus.Connected, "Connection established");
            UpdateLastActivity();

            return LinkOperationResult.CreateSuccess("Link connected successfully");
        }
        catch (Exception ex)
        {
            ChangeStatus(LinkStatus.Error, $"Connection failed: {ex.Message}");
            Health = LinkHealth.Bad(ex.Message, DateTime.UtcNow);
            
            AddDomainEvent(new LinkErrorOccurredEvent(Id, "CONNECTION_ERROR", ex.Message));
            return LinkOperationResult.CreateFailure($"Connection failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Disconnect the link
    /// </summary>
    public async Task<LinkOperationResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (Status == LinkStatus.Disconnected)
            return LinkOperationResult.CreateSuccess("Link already disconnected");

        try
        {
            ChangeStatus(LinkStatus.Disconnecting, "Disconnection initiated");
            
            if (Driver != null)
            {
                await Driver.ShutdownAsync(cancellationToken);
            }

            Health = LinkHealth.Unknown();
            ChangeStatus(LinkStatus.Disconnected, "Disconnection completed");
            UpdateLastActivity();

            return LinkOperationResult.CreateSuccess("Link disconnected successfully");
        }
        catch (Exception ex)
        {
            ChangeStatus(LinkStatus.Error, $"Disconnection failed: {ex.Message}");
            AddDomainEvent(new LinkErrorOccurredEvent(Id, "DISCONNECTION_ERROR", ex.Message));
            return LinkOperationResult.CreateFailure($"Disconnection failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Add a channel to this link
    /// </summary>
    public void AddChannel(Channel channel)
    {
        if (channel == null)
            throw new ArgumentNullException(nameof(channel));

        if (_channels.Any(c => c.Name == channel.Name))
            throw new InvalidOperationException($"Channel with name '{channel.Name}' already exists");

        if (_channels.Count >= Configuration.MaxChannels)
            throw new InvalidOperationException($"Maximum number of channels ({Configuration.MaxChannels}) reached");

        _channels.Add(channel);
        channel.AssignToLink(Id);
        
        AddDomainEvent(new LinkChannelAddedEvent(Id, channel.Id, channel.Name));
    }

    /// <summary>
    /// Remove a channel from this link
    /// </summary>
    public void RemoveChannel(Guid channelId)
    {
        var channel = _channels.FirstOrDefault(c => c.Id == channelId);
        if (channel == null)
            throw new InvalidOperationException($"Channel with ID '{channelId}' not found");

        _channels.Remove(channel);
        channel.UnassignFromLink();
        
        AddDomainEvent(new LinkChannelRemovedEvent(Id, channelId, channel.Name));
    }

    /// <summary>
    /// Perform health check on the link
    /// </summary>
    public async Task<LinkHealthResult> PerformHealthCheckAsync(CancellationToken cancellationToken = default)
    {
        if (Driver == null)
        {
            Health = LinkHealth.Unknown();
            return LinkHealthResult.CreateUnhealthy("No driver attached");
        }

        if (Status != LinkStatus.Connected)
        {
            Health = LinkHealth.Unknown();
            return LinkHealthResult.CreateUnhealthy($"Link not connected (Status: {Status.Name})");
        }

        try
        {
            var driverHealth = await Driver.HealthCheckAsync(cancellationToken);
            
            if (driverHealth.IsHealthy)
            {
                Health = LinkHealth.Good(DateTime.UtcNow);
                UpdateLastActivity();
                return LinkHealthResult.CreateHealthy($"Driver health: {driverHealth.Status}");
            }
            else
            {
                Health = LinkHealth.Bad(driverHealth.ErrorMessage ?? "Driver unhealthy", DateTime.UtcNow);
                return LinkHealthResult.CreateUnhealthy($"Driver unhealthy: {driverHealth.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Health = LinkHealth.Bad(ex.Message, DateTime.UtcNow);
            AddDomainEvent(new LinkErrorOccurredEvent(Id, "HEALTH_CHECK_ERROR", ex.Message));
            return LinkHealthResult.CreateUnhealthy($"Health check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Update link configuration
    /// </summary>
    public void UpdateConfiguration(LinkConfiguration newConfiguration)
    {
        if (newConfiguration == null)
            throw new ArgumentNullException(nameof(newConfiguration));

        if (Status == LinkStatus.Connected)
            throw new InvalidOperationException("Cannot update configuration while connected");

        var previousConfig = Configuration;
        Configuration = newConfiguration;
        
        AddDomainEvent(new LinkConfigurationUpdatedEvent(Id, previousConfig, newConfiguration));
    }

    /// <summary>
    /// Update link metadata
    /// </summary>
    public void UpdateMetadata(LinkMetadata newMetadata)
    {
        if (newMetadata == null)
            throw new ArgumentNullException(nameof(newMetadata));

        var previousMetadata = Metadata;
        Metadata = newMetadata;
        
        AddDomainEvent(new LinkMetadataUpdatedEvent(Id, previousMetadata, newMetadata));
    }

    private void ChangeStatus(LinkStatus newStatus, string? reason = null)
    {
        if (Status != newStatus)
        {
            var previousStatus = Status;
            Status = newStatus;
            
            AddDomainEvent(new LinkStatusChangedEvent(Id, previousStatus.Name, newStatus.Name, reason));
        }
    }

    private void UpdateLastActivity()
    {
        LastActivity = DateTime.UtcNow;
    }

    private void OnDriverStatusChanged(object? sender, DriverStatusChangedEventArgs e)
    {
        // Map driver status changes to link status changes
        var linkStatus = e.CurrentStatus.Name switch
        {
            "Ready" => LinkStatus.Disconnected,
            "Connected" => LinkStatus.Connected,
            "Error" => LinkStatus.Error,
            "Shutdown" => LinkStatus.Disconnected,
            _ => Status
        };

        if (linkStatus != Status)
        {
            ChangeStatus(linkStatus, $"Driver status changed: {e.CurrentStatus.Name}");
        }
    }

    private void OnDriverErrorOccurred(object? sender, DriverErrorEventArgs e)
    {
        AddDomainEvent(new LinkErrorOccurredEvent(Id, e.ErrorCode, e.ErrorMessage));
        
        if (Status == LinkStatus.Connected)
        {
            ChangeStatus(LinkStatus.Error, $"Driver error: {e.ErrorMessage}");
        }
    }
}