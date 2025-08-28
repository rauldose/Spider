using FluentValidation;
using Spider.DeviceManagement.Application.Commands;

namespace Spider.DeviceManagement.Application.Validators;

/// <summary>
/// Validator for CreateDeviceCommand
/// </summary>
public class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Device name is required")
            .MaximumLength(100)
            .WithMessage("Device name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Protocol)
            .NotEmpty()
            .WithMessage("Protocol is required");

        RuleFor(x => x.Host)
            .NotEmpty()
            .WithMessage("Host is required")
            .MaximumLength(255)
            .WithMessage("Host cannot exceed 255 characters");

        RuleFor(x => x.Port)
            .GreaterThan(0)
            .WithMessage("Port must be greater than 0")
            .LessThanOrEqualTo(65535)
            .WithMessage("Port must be less than or equal to 65535");

        RuleFor(x => x.Timeout)
            .GreaterThan(0)
            .WithMessage("Timeout must be greater than 0")
            .When(x => x.Timeout.HasValue);

        RuleFor(x => x.RetryCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Retry count cannot be negative")
            .When(x => x.RetryCount.HasValue);

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required");
    }
}