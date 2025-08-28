using Spider.Core.Application.Interfaces;
using Spider.DeviceManagement.Application.DTOs;

namespace Spider.DeviceManagement.Application.Queries;

/// <summary>
/// Query to get devices by project ID
/// </summary>
public class GetDevicesByProjectQuery : IQuery<IEnumerable<DeviceDto>>
{
    public Guid ProjectId { get; set; }
    
    public GetDevicesByProjectQuery(Guid projectId)
    {
        ProjectId = projectId;
    }
}