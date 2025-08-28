using Spider.Core.SharedKernel.Base;
using Spider.DataAcquisition.Domain.Enumerations;
using Spider.DataAcquisition.Domain.ValueObjects;
using Spider.DataAcquisition.Domain.Events;

namespace Spider.DataAcquisition.Domain.Entities;

/// <summary>
/// Represents a data point that can be acquired from a device
/// </summary>
public class DataPoint : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DataAddress Address { get; private set; }
    public DataType DataType { get; private set; }
    public Guid DeviceId { get; private set; }
    public bool IsEnabled { get; private set; }
    public int ScanInterval { get; private set; } // in milliseconds
    public DataValue? LastValue { get; private set; }
    public DateTime? LastScanTime { get; private set; }

    protected DataPoint() // For EF Core
    {
        Name = string.Empty;
        Address = new DataAddress("temp");
        DataType = DataType.String;
    }

    public DataPoint(
        Guid id,
        string name,
        DataAddress address,
        DataType dataType,
        Guid deviceId,
        string? description = null,
        int scanInterval = 1000)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        if (scanInterval <= 0)
            throw new ArgumentException("Scan interval must be greater than zero", nameof(scanInterval));

        Name = name;
        Description = description;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        DeviceId = deviceId;
        IsEnabled = true;
        ScanInterval = scanInterval;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        Name = name;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void UpdateScanInterval(int intervalMs)
    {
        if (intervalMs <= 0)
            throw new ArgumentException("Scan interval must be greater than zero", nameof(intervalMs));

        ScanInterval = intervalMs;
    }

    public void Enable()
    {
        if (!IsEnabled)
        {
            IsEnabled = true;
            PublishDomainEvent(new DataPointEnabledEvent(Id, Name));
        }
    }

    public void Disable()
    {
        if (IsEnabled)
        {
            IsEnabled = false;
            PublishDomainEvent(new DataPointDisabledEvent(Id, Name));
        }
    }

    public void UpdateValue(DataValue value)
    {
        var previousValue = LastValue;
        LastValue = value ?? throw new ArgumentNullException(nameof(value));
        LastScanTime = DateTime.UtcNow;

        PublishDomainEvent(new DataPointValueUpdatedEvent(Id, Name, previousValue, value));
    }
}