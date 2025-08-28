using Spider.Core.SharedKernel.Base;

namespace Spider.ProjectManagement.Domain.ValueObjects;

public class ProjectConfiguration : ValueObject
{
    public int MaxDevices { get; }
    public int MaxConnections { get; }
    public TimeSpan DataRetentionPeriod { get; }
    public bool EnableRealTimeMonitoring { get; }
    public bool EnableDataArchiving { get; }
    public bool EnableAlerting { get; }
    public Dictionary<string, string> CustomSettings { get; }

    public ProjectConfiguration(
        int maxDevices,
        int maxConnections,
        TimeSpan dataRetentionPeriod,
        bool enableRealTimeMonitoring,
        bool enableDataArchiving,
        bool enableAlerting,
        Dictionary<string, string>? customSettings = null)
    {
        if (maxDevices < 0)
            throw new ArgumentException("Max devices cannot be negative", nameof(maxDevices));
        
        if (maxConnections < 0)
            throw new ArgumentException("Max connections cannot be negative", nameof(maxConnections));
        
        if (dataRetentionPeriod < TimeSpan.Zero)
            throw new ArgumentException("Data retention period cannot be negative", nameof(dataRetentionPeriod));

        MaxDevices = maxDevices;
        MaxConnections = maxConnections;
        DataRetentionPeriod = dataRetentionPeriod;
        EnableRealTimeMonitoring = enableRealTimeMonitoring;
        EnableDataArchiving = enableDataArchiving;
        EnableAlerting = enableAlerting;
        CustomSettings = customSettings ?? new Dictionary<string, string>();
    }

    public static ProjectConfiguration Default() =>
        new(
            maxDevices: 100,
            maxConnections: 50,
            dataRetentionPeriod: TimeSpan.FromDays(30),
            enableRealTimeMonitoring: true,
            enableDataArchiving: true,
            enableAlerting: true);

    public static ProjectConfiguration Limited() =>
        new(
            maxDevices: 10,
            maxConnections: 5,
            dataRetentionPeriod: TimeSpan.FromDays(7),
            enableRealTimeMonitoring: true,
            enableDataArchiving: false,
            enableAlerting: false);

    public static ProjectConfiguration Enterprise() =>
        new(
            maxDevices: 1000,
            maxConnections: 500,
            dataRetentionPeriod: TimeSpan.FromDays(365),
            enableRealTimeMonitoring: true,
            enableDataArchiving: true,
            enableAlerting: true);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MaxDevices;
        yield return MaxConnections;
        yield return DataRetentionPeriod;
        yield return EnableRealTimeMonitoring;
        yield return EnableDataArchiving;
        yield return EnableAlerting;
        
        foreach (var setting in CustomSettings.OrderBy(x => x.Key))
        {
            yield return setting.Key;
            yield return setting.Value;
        }
    }
}