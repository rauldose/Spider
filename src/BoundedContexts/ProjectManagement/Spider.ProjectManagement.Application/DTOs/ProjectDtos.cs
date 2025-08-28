namespace Spider.ProjectManagement.Application.DTOs;

public record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    string Status,
    ProjectConfigurationDto Configuration,
    Guid? ParentProjectId,
    string? ParentProjectName,
    List<ProjectDto> ChildProjects,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy);

public record ProjectConfigurationDto(
    int MaxDevices,
    int MaxConnections,
    TimeSpan DataRetentionPeriod,
    bool EnableRealTimeMonitoring,
    bool EnableDataArchiving,
    bool EnableAlerting,
    Dictionary<string, string> CustomSettings);

public record CreateProjectDto(
    string Name,
    string Description,
    ProjectConfigurationDto? Configuration,
    Guid? ParentProjectId,
    string CreatedBy);

public record UpdateProjectDto(
    string Name,
    string Description,
    string ModifiedBy);

public record ProjectStatusChangeDto(
    string Status,
    string ModifiedBy,
    string? Reason);

public record ProjectConfigurationUpdateDto(
    ProjectConfigurationDto Configuration,
    string ModifiedBy);

public record ProjectSummaryDto(
    Guid Id,
    string Name,
    string Status,
    int DeviceCount,
    int ConnectionCount,
    DateTimeOffset CreatedAt,
    string? CreatedBy);

public record ProjectStatisticsDto(
    int TotalProjects,
    int ActiveProjects,
    int InactiveProjects,
    int ArchivedProjects,
    int TotalDevices,
    int TotalConnections);