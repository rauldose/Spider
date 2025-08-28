using Spider.Core.SharedKernel.Events;

namespace Spider.ProjectManagement.Domain.Events;

public class ProjectCreatedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public string Name { get; }
    public string Description { get; }
    public Guid? ParentProjectId { get; }
    public string CreatedBy { get; }

    public ProjectCreatedEvent(Guid projectId, string name, string description, Guid? parentProjectId, string createdBy)
    {
        ProjectId = projectId;
        Name = name;
        Description = description;
        ParentProjectId = parentProjectId;
        CreatedBy = createdBy;
    }
}

public class ProjectUpdatedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public string PreviousName { get; }
    public string NewName { get; }
    public string PreviousDescription { get; }
    public string NewDescription { get; }
    public string ModifiedBy { get; }

    public ProjectUpdatedEvent(Guid projectId, string previousName, string newName, string previousDescription, string newDescription, string modifiedBy)
    {
        ProjectId = projectId;
        PreviousName = previousName;
        NewName = newName;
        PreviousDescription = previousDescription;
        NewDescription = newDescription;
        ModifiedBy = modifiedBy;
    }
}

public class ProjectStatusChangedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public string ProjectName { get; }
    public string PreviousStatus { get; }
    public string NewStatus { get; }
    public string ModifiedBy { get; }
    public string? Reason { get; }

    public ProjectStatusChangedEvent(Guid projectId, string projectName, string previousStatus, string newStatus, string modifiedBy, string? reason)
    {
        ProjectId = projectId;
        ProjectName = projectName;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ModifiedBy = modifiedBy;
        Reason = reason;
    }
}

public class ProjectConfigurationUpdatedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public string ProjectName { get; }
    public object Configuration { get; }
    public string ModifiedBy { get; }

    public ProjectConfigurationUpdatedEvent(Guid projectId, string projectName, object configuration, string modifiedBy)
    {
        ProjectId = projectId;
        ProjectName = projectName;
        Configuration = configuration;
        ModifiedBy = modifiedBy;
    }
}

public class ProjectArchivedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public string ProjectName { get; }
    public string ArchivedBy { get; }
    public string? Reason { get; }

    public ProjectArchivedEvent(Guid projectId, string projectName, string archivedBy, string? reason)
    {
        ProjectId = projectId;
        ProjectName = projectName;
        ArchivedBy = archivedBy;
        Reason = reason;
    }
}

public class ProjectDeletedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public string ProjectName { get; }
    public string DeletedBy { get; }

    public ProjectDeletedEvent(Guid projectId, string projectName, string deletedBy)
    {
        ProjectId = projectId;
        ProjectName = projectName;
        DeletedBy = deletedBy;
    }
}