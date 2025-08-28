using Spider.Core.SharedKernel.Base;

namespace Spider.ProjectManagement.Domain.Enumerations;

public class ProjectStatus : Enumeration
{
    public static ProjectStatus Draft = new(1, nameof(Draft));
    public static ProjectStatus Active = new(2, nameof(Active));
    public static ProjectStatus Inactive = new(3, nameof(Inactive));
    public static ProjectStatus Archived = new(4, nameof(Archived));

    public ProjectStatus(int id, string name) : base(id, name)
    {
    }

    public static IEnumerable<ProjectStatus> List() =>
        new[] { Draft, Active, Inactive, Archived };

    public static ProjectStatus FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        return state ?? throw new ArgumentException($"Possible values for ProjectStatus: {string.Join(",", List().Select(s => s.Name))}");
    }

    public static ProjectStatus From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        return state ?? throw new ArgumentException($"Possible values for ProjectStatus: {string.Join(",", List().Select(s => s.Name))}");
    }
}