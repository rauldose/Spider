using Spider.Core.Application.Interfaces;

namespace Spider.Communication.Application.DTOs;

/// <summary>
/// Link Data Transfer Objects
/// </summary>
public record LinkDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ProtocolType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public LinkHealthDto Health { get; init; } = new();
    public LinkConfigurationDto Configuration { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime LastActivity { get; init; }
    public List<ChannelDto> Channels { get; init; } = new();
}

public record LinkHealthDto
{
    public bool IsHealthy { get; init; }
    public double SuccessRate { get; init; }
    public TimeSpan AverageResponseTime { get; init; }
    public int ErrorCount { get; init; }
    public DateTime LastError { get; init; }
    public string LastErrorMessage { get; init; } = string.Empty;
}

public record LinkConfigurationDto
{
    public string ConnectionString { get; init; } = string.Empty;
    public Dictionary<string, object> Parameters { get; init; } = new();
    public TimeSpan ConnectionTimeout { get; init; }
    public TimeSpan ReadTimeout { get; init; }
    public int MaxRetries { get; init; }
    public bool EnableHeartbeat { get; init; }
    public TimeSpan HeartbeatInterval { get; init; }
}

/// <summary>
/// Channel Data Transfer Objects
/// </summary>
public record ChannelDto
{
    public Guid Id { get; init; }
    public Guid LinkId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ChannelType { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<DataPointDto> DataPoints { get; init; } = new();
}

/// <summary>
/// DataPoint Data Transfer Objects
/// </summary>
public record DataPointDto
{
    public Guid Id { get; init; }
    public Guid ChannelId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public string AccessMode { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
    public object? CurrentValue { get; init; }
    public string Quality { get; init; } = string.Empty;
    public DateTime? LastUpdated { get; init; }
}

/// <summary>
/// Command DTOs for creating/updating entities
/// </summary>
public record CreateLinkDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ProtocolType { get; init; } = string.Empty;
    public LinkConfigurationDto Configuration { get; init; } = new();
}

public record UpdateLinkDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public LinkConfigurationDto Configuration { get; init; } = new();
}

public record CreateChannelDto
{
    public Guid LinkId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ChannelType { get; init; } = string.Empty;
}

public record CreateDataPointDto
{
    public Guid ChannelId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public string AccessMode { get; init; } = string.Empty;
}

/// <summary>
/// Statistics and monitoring DTOs
/// </summary>
public record CommunicationStatisticsDto
{
    public int TotalLinks { get; init; }
    public int ConnectedLinks { get; init; }
    public int DisconnectedLinks { get; init; }
    public int HealthyLinks { get; init; }
    public int TotalChannels { get; init; }
    public int ActiveChannels { get; init; }
    public int TotalDataPoints { get; init; }
    public int ActiveDataPoints { get; init; }
    public double OverallSuccessRate { get; init; }
    public TimeSpan AverageResponseTime { get; init; }
}