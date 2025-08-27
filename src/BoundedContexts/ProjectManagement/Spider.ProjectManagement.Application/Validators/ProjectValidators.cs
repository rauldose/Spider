using FluentValidation;
using Spider.ProjectManagement.Application.Commands;

namespace Spider.ProjectManagement.Application.Validators;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required")
            .MaximumLength(100)
            .WithMessage("Project name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Project description is required")
            .MaximumLength(500)
            .WithMessage("Project description cannot exceed 500 characters");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("CreatedBy is required");
    }
}

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Project ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required")
            .MaximumLength(100)
            .WithMessage("Project name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Project description is required")
            .MaximumLength(500)
            .WithMessage("Project description cannot exceed 500 characters");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty()
            .WithMessage("ModifiedBy is required");
    }
}

public class ChangeProjectStatusCommandValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    public ChangeProjectStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Project ID is required");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(BeValidStatus)
            .WithMessage("Status must be one of: Draft, Active, Inactive, Archived");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty()
            .WithMessage("ModifiedBy is required");
    }

    private static bool BeValidStatus(string status)
    {
        var validStatuses = new[] { "Draft", "Active", "Inactive", "Archived" };
        return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}