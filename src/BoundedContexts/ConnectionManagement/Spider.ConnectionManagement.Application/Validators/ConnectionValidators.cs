using FluentValidation;
using Spider.ConnectionManagement.Application.Commands;

namespace Spider.ConnectionManagement.Application.Validators;

public class CreateConnectionCommandValidator : AbstractValidator<CreateConnectionCommand>
{
    public CreateConnectionCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("Device ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Connection name is required")
            .MaximumLength(100)
            .WithMessage("Connection name must not exceed 100 characters");

        RuleFor(x => x.Protocol)
            .NotEmpty()
            .WithMessage("Protocol is required")
            .Must(BeValidProtocol)
            .WithMessage("Invalid protocol. Supported protocols: Modbus, OpcUa, Mqtt, EthernetIp, Siemens, Omron, Mitsubishi");

        RuleFor(x => x.Host)
            .NotEmpty()
            .WithMessage("Host is required")
            .MaximumLength(255)
            .WithMessage("Host must not exceed 255 characters");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .WithMessage("Port must be between 1 and 65535");

        RuleFor(x => x.TimeoutMs)
            .GreaterThan(0)
            .WithMessage("Timeout must be greater than 0")
            .LessThanOrEqualTo(300000)
            .WithMessage("Timeout must not exceed 5 minutes (300000ms)");

        RuleFor(x => x.RetryAttempts)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Retry attempts cannot be negative")
            .LessThanOrEqualTo(10)
            .WithMessage("Retry attempts must not exceed 10");
    }

    private static bool BeValidProtocol(string protocol)
    {
        var validProtocols = new[] { "Modbus", "OpcUa", "Mqtt", "EthernetIp", "Siemens", "Omron", "Mitsubishi" };
        return validProtocols.Contains(protocol, StringComparer.OrdinalIgnoreCase);
    }
}

public class UpdateConnectionParametersCommandValidator : AbstractValidator<UpdateConnectionParametersCommand>
{
    public UpdateConnectionParametersCommandValidator()
    {
        RuleFor(x => x.ConnectionId)
            .NotEmpty()
            .WithMessage("Connection ID is required");

        RuleFor(x => x.Host)
            .NotEmpty()
            .WithMessage("Host is required")
            .MaximumLength(255)
            .WithMessage("Host must not exceed 255 characters");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .WithMessage("Port must be between 1 and 65535");

        RuleFor(x => x.TimeoutMs)
            .GreaterThan(0)
            .WithMessage("Timeout must be greater than 0")
            .LessThanOrEqualTo(300000)
            .WithMessage("Timeout must not exceed 5 minutes (300000ms)");

        RuleFor(x => x.RetryAttempts)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Retry attempts cannot be negative")
            .LessThanOrEqualTo(10)
            .WithMessage("Retry attempts must not exceed 10");
    }
}

public class TestConnectionCommandValidator : AbstractValidator<TestConnectionCommand>
{
    public TestConnectionCommandValidator()
    {
        RuleFor(x => x.Protocol)
            .NotEmpty()
            .WithMessage("Protocol is required")
            .Must(BeValidProtocol)
            .WithMessage("Invalid protocol. Supported protocols: Modbus, OpcUa, Mqtt, EthernetIp, Siemens, Omron, Mitsubishi");

        RuleFor(x => x.Host)
            .NotEmpty()
            .WithMessage("Host is required")
            .MaximumLength(255)
            .WithMessage("Host must not exceed 255 characters");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .WithMessage("Port must be between 1 and 65535");

        RuleFor(x => x.TimeoutMs)
            .GreaterThan(0)
            .WithMessage("Timeout must be greater than 0")
            .LessThanOrEqualTo(30000)
            .WithMessage("Test timeout must not exceed 30 seconds");
    }

    private static bool BeValidProtocol(string protocol)
    {
        var validProtocols = new[] { "Modbus", "OpcUa", "Mqtt", "EthernetIp", "Siemens", "Omron", "Mitsubishi" };
        return validProtocols.Contains(protocol, StringComparer.OrdinalIgnoreCase);
    }
}