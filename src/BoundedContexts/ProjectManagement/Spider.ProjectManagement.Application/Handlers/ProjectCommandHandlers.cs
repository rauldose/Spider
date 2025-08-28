using MediatR;
using Spider.Core.SharedKernel.Abstractions;
using Spider.ProjectManagement.Application.Commands;
using Spider.ProjectManagement.Application.DTOs;
using Spider.ProjectManagement.Domain.Entities;
using Spider.ProjectManagement.Domain.Enumerations;
using Spider.ProjectManagement.Domain.ValueObjects;

namespace Spider.ProjectManagement.Application.Handlers;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectCommandHandler(
        IRepository<Project, Guid> projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var configuration = request.Configuration != null
            ? new ProjectConfiguration(
                request.Configuration.MaxDevices,
                request.Configuration.MaxConnections,
                request.Configuration.DataRetentionPeriod,
                request.Configuration.EnableRealTimeMonitoring,
                request.Configuration.EnableDataArchiving,
                request.Configuration.EnableAlerting,
                request.Configuration.CustomSettings)
            : ProjectConfiguration.Default();

        var project = Project.Create(
            request.Name,
            request.Description,
            configuration,
            request.ParentProjectId,
            request.CreatedBy);

        await _projectRepository.AddAsync(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMappingHelper.MapToDto(project);
}

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(
        IRepository<Project, Guid> projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.Id} not found");

        project.UpdateDetails(request.Name, request.Description, request.ModifiedBy);

        await _projectRepository.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMappingHelper.MapToDto(project);
    }
}

public class ChangeProjectStatusCommandHandler : IRequestHandler<ChangeProjectStatusCommand, ProjectDto>
{
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeProjectStatusCommandHandler(
        IRepository<Project, Guid> projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDto> Handle(ChangeProjectStatusCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.Id} not found");

        var newStatus = ProjectStatus.FromName(request.Status);
        project.ChangeStatus(newStatus, request.ModifiedBy, request.Reason);

        await _projectRepository.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMappingHelper.MapToDto(project);
    }
}

public class ActivateProjectCommandHandler : IRequestHandler<ActivateProjectCommand, ProjectDto>
{
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateProjectCommandHandler(
        IRepository<Project, Guid> projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDto> Handle(ActivateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.Id} not found");

        project.Activate(request.ModifiedBy);

        await _projectRepository.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMappingHelper.MapToDto(project);
    }
}

public class DeactivateProjectCommandHandler : IRequestHandler<DeactivateProjectCommand, ProjectDto>
{
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateProjectCommandHandler(
        IRepository<Project, Guid> projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDto> Handle(DeactivateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.Id} not found");

        project.Deactivate(request.ModifiedBy, request.Reason);

        await _projectRepository.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMappingHelper.MapToDto(project);
    }
}

public class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, ProjectDto>
{
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveProjectCommandHandler(
        IRepository<Project, Guid> projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDto> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.Id} not found");

        project.Archive(request.ModifiedBy, request.Reason);

        await _projectRepository.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMappingHelper.MapToDto(project);
    }
}

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
{
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProjectCommandHandler(
        IRepository<Project, Guid> projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.Id} not found");

        if (!project.CanBeDeleted())
            throw new InvalidOperationException("Project cannot be deleted. It must be in Draft or Archived status and have no child projects.");

        await _projectRepository.RemoveAsync(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
}