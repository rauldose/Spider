using Spider.Core.SharedKernel.Abstractions;
using Spider.ProjectManagement.Domain.Entities;
using System.Linq.Expressions;

namespace Spider.ProjectManagement.Domain.Specifications;

public class ProjectsByStatusSpecification : ISpecification<Project>
{
    public ProjectsByStatusSpecification(string status)
    {
        Criteria = project => project.Status.Name == status;
        OrderBy = project => project.Name;
    }

    public Expression<Func<Project, bool>>? Criteria { get; }
    public List<Expression<Func<Project, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Project, object>>? OrderBy { get; }
    public Expression<Func<Project, object>>? OrderByDescending { get; }
    public Expression<Func<Project, object>>? GroupBy { get; }
    public int? Take { get; }
    public int? Skip { get; }
    public bool IsPagingEnabled => false;
}

public class ProjectsByParentSpecification : ISpecification<Project>
{
    public ProjectsByParentSpecification(Guid? parentProjectId)
    {
        Criteria = project => project.ParentProjectId == parentProjectId;
        OrderBy = project => project.Name;
    }

    public Expression<Func<Project, bool>>? Criteria { get; }
    public List<Expression<Func<Project, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Project, object>>? OrderBy { get; }
    public Expression<Func<Project, object>>? OrderByDescending { get; }
    public Expression<Func<Project, object>>? GroupBy { get; }
    public int? Take { get; }
    public int? Skip { get; }
    public bool IsPagingEnabled => false;
}

public class ProjectsByCreatorSpecification : ISpecification<Project>
{
    public ProjectsByCreatorSpecification(string createdBy)
    {
        Criteria = project => project.CreatedBy == createdBy;
        OrderBy = project => project.CreatedAt;
    }

    public Expression<Func<Project, bool>>? Criteria { get; }
    public List<Expression<Func<Project, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Project, object>>? OrderBy { get; }
    public Expression<Func<Project, object>>? OrderByDescending { get; }
    public Expression<Func<Project, object>>? GroupBy { get; }
    public int? Take { get; }
    public int? Skip { get; }
    public bool IsPagingEnabled => false;
}

public class ActiveProjectsSpecification : ISpecification<Project>
{
    public ActiveProjectsSpecification()
    {
        Criteria = project => project.Status.Name == "Active";
        OrderBy = project => project.Name;
    }

    public Expression<Func<Project, bool>>? Criteria { get; }
    public List<Expression<Func<Project, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Project, object>>? OrderBy { get; }
    public Expression<Func<Project, object>>? OrderByDescending { get; }
    public Expression<Func<Project, object>>? GroupBy { get; }
    public int? Take { get; }
    public int? Skip { get; }
    public bool IsPagingEnabled => false;
}

public class RootProjectsSpecification : ISpecification<Project>
{
    public RootProjectsSpecification()
    {
        Criteria = project => project.ParentProjectId == null;
        OrderBy = project => project.Name;
    }

    public Expression<Func<Project, bool>>? Criteria { get; }
    public List<Expression<Func<Project, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Project, object>>? OrderBy { get; }
    public Expression<Func<Project, object>>? OrderByDescending { get; }
    public Expression<Func<Project, object>>? GroupBy { get; }
    public int? Take { get; }
    public int? Skip { get; }
    public bool IsPagingEnabled => false;
}