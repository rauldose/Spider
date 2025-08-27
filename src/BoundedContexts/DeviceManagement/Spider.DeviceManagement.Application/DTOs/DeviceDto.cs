namespace Spider.DeviceManagement.Application.DTOs;

/// <summary>
/// Data transfer object for device information
/// </summary>
public class DeviceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string? LastError { get; set; }
    public DateTimeOffset? LastConnectedAt { get; set; }
    public Guid ProjectId { get; set; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}