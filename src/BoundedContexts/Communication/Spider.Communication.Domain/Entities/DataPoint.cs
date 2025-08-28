using Spider.Core.SharedKernel.Base;
using Spider.Communication.Domain.ValueObjects;
using Spider.Communication.Domain.Events;

namespace Spider.Communication.Domain.Entities;

/// <summary>
/// DataPoint entity representing a single data point in a communication channel
/// </summary>
public class DataPoint : Entity<Guid>
{
    public string Address { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DataPointType DataType { get; private set; }
    public int? Length { get; private set; }
    public bool IsWritable { get; private set; }
    public object? CurrentValue { get; private set; }
    public string? DataQuality { get; private set; }
    public DateTime? LastUpdated { get; private set; }
    public Guid? ChannelId { get; private set; }
    public DataPointConfiguration Configuration { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Private constructor for EF Core
    private DataPoint() { }

    /// <summary>
    /// Create a new DataPoint
    /// </summary>
    public DataPoint(
        string address, 
        string name, 
        string description, 
        DataPointType dataType, 
        bool isWritable = false,
        int? length = null,
        DataPointConfiguration? configuration = null) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty", nameof(address));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Address = address;
        Name = name;
        Description = description ?? string.Empty;
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        Length = length;
        IsWritable = isWritable;
        Configuration = configuration ?? DataPointConfiguration.Default();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assign this data point to a channel
    /// </summary>
    internal void AssignToChannel(Guid channelId)
    {
        if (ChannelId.HasValue)
            throw new InvalidOperationException($"Data point is already assigned to channel {ChannelId}");

        ChannelId = channelId;
    }

    /// <summary>
    /// Unassign this data point from its current channel
    /// </summary>
    internal void UnassignFromChannel()
    {
        ChannelId = null;
    }

    /// <summary>
    /// Update the data point value with timestamp and quality
    /// </summary>
    public void UpdateValue(object? value, string quality, DateTime timestamp)
    {
        var previousValue = CurrentValue;
        CurrentValue = value;
        DataQuality = quality;
        LastUpdated = timestamp;
    }

    /// <summary>
    /// Update data point configuration
    /// </summary>
    public void UpdateConfiguration(DataPointConfiguration newConfiguration)
    {
        Configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
    }

    /// <summary>
    /// Update data point metadata
    /// </summary>
    public void UpdateMetadata(string newName, string newDescription, bool newIsWritable)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty", nameof(newName));

        Name = newName;
        Description = newDescription ?? string.Empty;
        IsWritable = newIsWritable;
    }
}