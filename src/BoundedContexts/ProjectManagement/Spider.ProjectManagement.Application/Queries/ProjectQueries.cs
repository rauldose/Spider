using MediatR;
using Spider.ProjectManagement.Application.DTOs;

namespace Spider.ProjectManagement.Application.Queries;

public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto?>;

public record GetProjectsByParentQuery(Guid? ParentProjectId) : IRequest<List<ProjectDto>>;

public record GetProjectsByStatusQuery(string Status) : IRequest<List<ProjectDto>>;

public record GetProjectsByCreatorQuery(string CreatedBy) : IRequest<List<ProjectDto>>;

public record GetAllProjectsQuery() : IRequest<List<ProjectDto>>;

public record GetRootProjectsQuery() : IRequest<List<ProjectDto>>;

public record GetActiveProjectsQuery() : IRequest<List<ProjectDto>>;

public record GetProjectHierarchyQuery(Guid RootProjectId) : IRequest<ProjectDto?>;

public record GetProjectStatisticsQuery() : IRequest<ProjectStatisticsDto>;

public record GetProjectSummariesQuery() : IRequest<List<ProjectSummaryDto>>;