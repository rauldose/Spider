using Spider.Core.SharedKernel.Base;
using Spider.ProjectManagement.Domain.Events;
using Spider.ProjectManagement.Domain.ValueObjects;
using Spider.ProjectManagement.Domain.Enumerations;

namespace Spider.ProjectManagement.Domain.Entities;

public class Project : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public ProjectConfiguration Configuration { get; private set; }
    public Guid? ParentProjectId { get; private set; }
    public Project? ParentProject { get; private set; }
    public List<Project> ChildProjects { get; private set; } = new();
    public DateTime LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // For EF Core
    private Project() : base()
    {
        Name = string.Empty;
        Description = string.Empty;
        Status = ProjectStatus.Draft;
        Configuration = ProjectConfiguration.Default();
    }

    private Project(
        Guid id,
        string name,
        string description,
        ProjectConfiguration configuration,
        Guid? parentProjectId,
        string createdBy) : base(id)
    {
        Name = name;
        Description = description;
        Status = ProjectStatus.Draft;
        Configuration = configuration;
        ParentProjectId = parentProjectId;
        CreatedBy = createdBy;

        AddDomainEvent(new ProjectCreatedEvent(Id, Name, Description, ParentProjectId, CreatedBy!));
    }

    public static Project Create(
        string name,
        string description,
        ProjectConfiguration configuration,
        Guid? parentProjectId,
        string createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        return new Project(Guid.NewGuid(), name, description, configuration, parentProjectId, createdBy);
    }

    public void UpdateDetails(string name, string description, string modifiedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(modifiedBy);

        var previousName = Name;
        var previousDescription = Description;

        Name = name;
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        UpdatedBy = modifiedBy;

        AddDomainEvent(new ProjectUpdatedEvent(Id, previousName, Name, previousDescription, Description, modifiedBy));
    }

    public void ChangeStatus(ProjectStatus newStatus, string modifiedBy, string? reason = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modifiedBy);

        if (Status != newStatus)
        {
            var previousStatus = Status;
            Status = newStatus;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
            UpdatedBy = modifiedBy;

            AddDomainEvent(new ProjectStatusChangedEvent(Id, Name, previousStatus.Name, newStatus.Name, modifiedBy, reason));
        }
    }

    public void UpdateConfiguration(ProjectConfiguration configuration, string modifiedBy)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(modifiedBy);

        Configuration = configuration;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        UpdatedBy = modifiedBy;

        AddDomainEvent(new ProjectConfigurationUpdatedEvent(Id, Name, configuration, modifiedBy));
    }

    public void Activate(string modifiedBy)
    {
        if (Status == ProjectStatus.Draft)
        {
            ChangeStatus(ProjectStatus.Active, modifiedBy, "Project activated");
        }
        else if (Status == ProjectStatus.Inactive)
        {
            ChangeStatus(ProjectStatus.Active, modifiedBy, "Project reactivated");
        }
    }

    public void Deactivate(string modifiedBy, string? reason = null)
    {
        if (Status == ProjectStatus.Active)
        {
            ChangeStatus(ProjectStatus.Inactive, modifiedBy, reason ?? "Project deactivated");
        }
    }

    public void Archive(string modifiedBy, string? reason = null)
    {
        if (Status != ProjectStatus.Archived)
        {
            ChangeStatus(ProjectStatus.Archived, modifiedBy, reason ?? "Project archived");
        }
    }

    public bool CanHaveChildren()
    {
        return Status == ProjectStatus.Active || Status == ProjectStatus.Inactive;
    }

    public bool CanBeDeleted()
    {
        return !ChildProjects.Any() && (Status == ProjectStatus.Draft || Status == ProjectStatus.Archived);
    }
}