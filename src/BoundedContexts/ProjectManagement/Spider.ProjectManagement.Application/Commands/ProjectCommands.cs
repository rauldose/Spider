using MediatR;
using Spider.ProjectManagement.Application.DTOs;

namespace Spider.ProjectManagement.Application.Commands;

public record CreateProjectCommand(
    string Name,
    string Description,
    ProjectConfigurationDto? Configuration,
    Guid? ParentProjectId,
    string CreatedBy) : IRequest<ProjectDto>;

public record UpdateProjectCommand(
    Guid Id,
    string Name,
    string Description,
    string ModifiedBy) : IRequest<ProjectDto>;

public record ChangeProjectStatusCommand(
    Guid Id,
    string Status,
    string ModifiedBy,
    string? Reason = null) : IRequest<ProjectDto>;

public record UpdateProjectConfigurationCommand(
    Guid Id,
    ProjectConfigurationDto Configuration,
    string ModifiedBy) : IRequest<ProjectDto>;

public record ActivateProjectCommand(
    Guid Id,
    string ModifiedBy) : IRequest<ProjectDto>;

public record DeactivateProjectCommand(
    Guid Id,
    string ModifiedBy,
    string? Reason = null) : IRequest<ProjectDto>;

public record ArchiveProjectCommand(
    Guid Id,
    string ModifiedBy,
    string? Reason = null) : IRequest<ProjectDto>;

public record DeleteProjectCommand(
    Guid Id,
    string DeletedBy) : IRequest;