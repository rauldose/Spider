using FluentValidation;
using Spider.DataAcquisition.Application.Commands;

namespace Spider.DataAcquisition.Application.Validators;

/// <summary>
/// Validator for create data point command
/// </summary>
public class CreateDataPointCommandValidator : AbstractValidator<CreateDataPointCommand>
{
    public CreateDataPointCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DataType)
            .NotEmpty()
            .Must(BeValidDataType)
            .WithMessage("Invalid data type. Valid types are: Boolean, Integer, Double, String, DateTime, Binary");

        RuleFor(x => x.DeviceId)
            .NotEmpty();

        RuleFor(x => x.ScanInterval)
            .GreaterThan(0)
            .LessThanOrEqualTo(3600000); // Max 1 hour

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }

    private static bool BeValidDataType(string dataType)
    {
        var validTypes = new[] { "Boolean", "Integer", "Double", "String", "DateTime", "Binary" };
        return validTypes.Contains(dataType, StringComparer.OrdinalIgnoreCase);
    }
}