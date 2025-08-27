using Spider.Core.Application.Interfaces;

namespace Spider.DeviceManagement.Application.Commands;

/// <summary>
/// Command to create a new device
/// </summary>
public class CreateDeviceCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public int? Timeout { get; set; }
    public int? RetryCount { get; set; }
    public Dictionary<string, string> AdditionalParameters { get; set; } = new();
    public Guid ProjectId { get; set; }
}