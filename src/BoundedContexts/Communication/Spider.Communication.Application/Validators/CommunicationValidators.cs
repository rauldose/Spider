using FluentValidation;
using Spider.Communication.Application.Commands;

namespace Spider.Communication.Application.Validators;

/// <summary>
/// Link Command Validators
/// </summary>
public class CreateLinkCommandValidator : AbstractValidator<CreateLinkCommand>
{
    public CreateLinkCommandValidator()
    {
        RuleFor(x => x.LinkDto.Name)
            .NotEmpty().WithMessage("Link name is required")
            .MaximumLength(100).WithMessage("Link name cannot exceed 100 characters");

        RuleFor(x => x.LinkDto.ProtocolType)
            .NotEmpty().WithMessage("Protocol type is required")
            .Must(BeValidProtocolType).WithMessage("Invalid protocol type");

        RuleFor(x => x.LinkDto.Configuration.ConnectionString)
            .NotEmpty().WithMessage("Connection string is required");

        RuleFor(x => x.LinkDto.Configuration.ConnectionTimeout)
            .GreaterThan(TimeSpan.Zero).WithMessage("Connection timeout must be greater than zero")
            .LessThan(TimeSpan.FromMinutes(10)).WithMessage("Connection timeout cannot exceed 10 minutes");
    }

    private static bool BeValidProtocolType(string protocolType)
    {
        var validProtocols = new[] { "Modbus", "OpcUa", "Mqtt", "EtherNetIP", "Siemens", "Omron", "Mitsubishi" };
        return validProtocols.Contains(protocolType, StringComparer.OrdinalIgnoreCase);
    }
}

public class UpdateLinkCommandValidator : AbstractValidator<UpdateLinkCommand>
{
    public UpdateLinkCommandValidator()
    {
        RuleFor(x => x.LinkDto.Id)
            .NotEmpty().WithMessage("Link ID is required");

        RuleFor(x => x.LinkDto.Name)
            .NotEmpty().WithMessage("Link name is required")
            .MaximumLength(100).WithMessage("Link name cannot exceed 100 characters");

        RuleFor(x => x.LinkDto.Configuration.ConnectionTimeout)
            .GreaterThan(TimeSpan.Zero).WithMessage("Connection timeout must be greater than zero")
            .LessThan(TimeSpan.FromMinutes(10)).WithMessage("Connection timeout cannot exceed 10 minutes");
    }
}

public class ConnectLinkCommandValidator : AbstractValidator<ConnectLinkCommand>
{
    public ConnectLinkCommandValidator()
    {
        RuleFor(x => x.LinkId)
            .NotEmpty().WithMessage("Link ID is required");
    }
}

public class AttachDriverToLinkCommandValidator : AbstractValidator<AttachDriverToLinkCommand>
{
    public AttachDriverToLinkCommandValidator()
    {
        RuleFor(x => x.LinkId)
            .NotEmpty().WithMessage("Link ID is required");

        RuleFor(x => x.DriverType)
            .NotEmpty().WithMessage("Driver type is required");

        RuleFor(x => x.DriverConfiguration)
            .NotNull().WithMessage("Driver configuration is required");
    }
}

/// <summary>
/// Channel Command Validators
/// </summary>
public class CreateChannelCommandValidator : AbstractValidator<CreateChannelCommand>
{
    public CreateChannelCommandValidator()
    {
        RuleFor(x => x.ChannelDto.LinkId)
            .NotEmpty().WithMessage("Link ID is required");

        RuleFor(x => x.ChannelDto.Name)
            .NotEmpty().WithMessage("Channel name is required")
            .MaximumLength(100).WithMessage("Channel name cannot exceed 100 characters");

        RuleFor(x => x.ChannelDto.ChannelType)
            .NotEmpty().WithMessage("Channel type is required");
    }
}

public class UpdateChannelCommandValidator : AbstractValidator<UpdateChannelCommand>
{
    public UpdateChannelCommandValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty().WithMessage("Channel ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Channel name is required")
            .MaximumLength(100).WithMessage("Channel name cannot exceed 100 characters");

        RuleFor(x => x.ChannelType)
            .NotEmpty().WithMessage("Channel type is required");
    }
}

/// <summary>
/// DataPoint Command Validators
/// </summary>
public class CreateDataPointCommandValidator : AbstractValidator<CreateDataPointCommand>
{
    public CreateDataPointCommandValidator()
    {
        RuleFor(x => x.DataPointDto.ChannelId)
            .NotEmpty().WithMessage("Channel ID is required");

        RuleFor(x => x.DataPointDto.Name)
            .NotEmpty().WithMessage("DataPoint name is required")
            .MaximumLength(100).WithMessage("DataPoint name cannot exceed 100 characters");

        RuleFor(x => x.DataPointDto.Address)
            .NotEmpty().WithMessage("DataPoint address is required");

        RuleFor(x => x.DataPointDto.DataType)
            .NotEmpty().WithMessage("Data type is required")
            .Must(BeValidDataType).WithMessage("Invalid data type");

        RuleFor(x => x.DataPointDto.AccessMode)
            .NotEmpty().WithMessage("Access mode is required")
            .Must(BeValidAccessMode).WithMessage("Invalid access mode");
    }

    private static bool BeValidDataType(string dataType)
    {
        var validTypes = new[] { "Boolean", "Byte", "Int16", "UInt16", "Int32", "UInt32", "Int64", "UInt64", "Float", "Double", "String" };
        return validTypes.Contains(dataType, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidAccessMode(string accessMode)
    {
        var validModes = new[] { "ReadOnly", "WriteOnly", "ReadWrite" };
        return validModes.Contains(accessMode, StringComparer.OrdinalIgnoreCase);
    }
}

public class WriteDataPointCommandValidator : AbstractValidator<WriteDataPointCommand>
{
    public WriteDataPointCommandValidator()
    {
        RuleFor(x => x.DataPointId)
            .NotEmpty().WithMessage("DataPoint ID is required");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Value is required");
    }
}