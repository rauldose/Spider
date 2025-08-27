using MediatR;
using Spider.Core.SharedKernel.Abstractions;
using Spider.ProjectManagement.Application.DTOs;
using Spider.ProjectManagement.Application.Queries;
using Spider.ProjectManagement.Domain.Entities;
using Spider.ProjectManagement.Domain.Specifications;

namespace Spider.ProjectManagement.Application.Handlers;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    private readonly IRepository<Project, Guid> _projectRepository;

    public GetProjectByIdQueryHandler(IRepository<Project, Guid> projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id);
        return project != null ? MapToDto(project) : null;
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Status.Name,
            new ProjectConfigurationDto(
                project.Configuration.MaxDevices,
                project.Configuration.MaxConnections,
                project.Configuration.DataRetentionPeriod,
                project.Configuration.EnableRealTimeMonitoring,
                project.Configuration.EnableDataArchiving,
                project.Configuration.EnableAlerting,
                project.Configuration.CustomSettings),
            project.ParentProjectId,
            project.ParentProject?.Name,
            project.ChildProjects.Select(MapToDto).ToList(),
            project.CreatedAt,
            project.LastModifiedAt,
            project.CreatedBy,
            project.LastModifiedBy);
    }
}

public class GetProjectsByParentQueryHandler : IRequestHandler<GetProjectsByParentQuery, List<ProjectDto>>
{
    private readonly IRepository<Project, Guid> _projectRepository;

    public GetProjectsByParentQueryHandler(IRepository<Project, Guid> projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsByParentQuery request, CancellationToken cancellationToken)
    {
        var specification = new ProjectsByParentSpecification(request.ParentProjectId);
        var projects = await _projectRepository.FindAsync(specification);
        return projects.Select(MapToDto).ToList();
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Status.Name,
            new ProjectConfigurationDto(
                project.Configuration.MaxDevices,
                project.Configuration.MaxConnections,
                project.Configuration.DataRetentionPeriod,
                project.Configuration.EnableRealTimeMonitoring,
                project.Configuration.EnableDataArchiving,
                project.Configuration.EnableAlerting,
                project.Configuration.CustomSettings),
            project.ParentProjectId,
            project.ParentProject?.Name,
            project.ChildProjects.Select(MapToDto).ToList(),
            project.CreatedAt,
            project.LastModifiedAt,
            project.CreatedBy,
            project.LastModifiedBy);
    }
}

public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, List<ProjectDto>>
{
    private readonly IRepository<Project, Guid> _projectRepository;

    public GetAllProjectsQueryHandler(IRepository<Project, Guid> projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<List<ProjectDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _projectRepository.GetAllAsync();
        return projects.Select(MapToDto).ToList();
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Status.Name,
            new ProjectConfigurationDto(
                project.Configuration.MaxDevices,
                project.Configuration.MaxConnections,
                project.Configuration.DataRetentionPeriod,
                project.Configuration.EnableRealTimeMonitoring,
                project.Configuration.EnableDataArchiving,
                project.Configuration.EnableAlerting,
                project.Configuration.CustomSettings),
            project.ParentProjectId,
            project.ParentProject?.Name,
            project.ChildProjects.Select(MapToDto).ToList(),
            project.CreatedAt,
            project.LastModifiedAt,
            project.CreatedBy,
            project.LastModifiedBy);
    }
}

public class GetProjectStatisticsQueryHandler : IRequestHandler<GetProjectStatisticsQuery, ProjectStatisticsDto>
{
    private readonly IRepository<Project, Guid> _projectRepository;

    public GetProjectStatisticsQueryHandler(IRepository<Project, Guid> projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectStatisticsDto> Handle(GetProjectStatisticsQuery request, CancellationToken cancellationToken)
    {
        var allProjects = (await _projectRepository.GetAllAsync()).ToList();

        var totalProjects = allProjects.Count;
        var activeProjects = allProjects.Count(p => p.Status.Name == "Active");
        var inactiveProjects = allProjects.Count(p => p.Status.Name == "Inactive");
        var archivedProjects = allProjects.Count(p => p.Status.Name == "Archived");

        // For now, return 0 for devices and connections
        // These would be calculated by querying the respective bounded contexts
        return new ProjectStatisticsDto(
            totalProjects,
            activeProjects,
            inactiveProjects,
            archivedProjects,
            0, // TotalDevices - would come from DeviceManagement
            0  // TotalConnections - would come from ConnectionManagement
        );
    }
}

public class GetRootProjectsQueryHandler : IRequestHandler<GetRootProjectsQuery, List<ProjectDto>>
{
    private readonly IRepository<Project, Guid> _projectRepository;

    public GetRootProjectsQueryHandler(IRepository<Project, Guid> projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<List<ProjectDto>> Handle(GetRootProjectsQuery request, CancellationToken cancellationToken)
    {
        var specification = new RootProjectsSpecification();
        var projects = await _projectRepository.FindAsync(specification);
        return projects.Select(MapToDto).ToList();
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Status.Name,
            new ProjectConfigurationDto(
                project.Configuration.MaxDevices,
                project.Configuration.MaxConnections,
                project.Configuration.DataRetentionPeriod,
                project.Configuration.EnableRealTimeMonitoring,
                project.Configuration.EnableDataArchiving,
                project.Configuration.EnableAlerting,
                project.Configuration.CustomSettings),
            project.ParentProjectId,
            project.ParentProject?.Name,
            project.ChildProjects.Select(MapToDto).ToList(),
            project.CreatedAt,
            project.LastModifiedAt,
            project.CreatedBy,
            project.LastModifiedBy);
    }
}