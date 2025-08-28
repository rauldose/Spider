using AutoMapper;
using Spider.Communication.Application.DTOs;
using Spider.Communication.Domain.Entities;
using Spider.Communication.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Spider.Communication.Application.Mappings;

/// <summary>
/// AutoMapper profile for Communication bounded context
/// </summary>
public class CommunicationMappingProfile : Profile
{
    public CommunicationMappingProfile()
    {
        CreateMap<Link, LinkDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Metadata.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Metadata.Description))
            .ForMember(dest => dest.ProtocolType, opt => opt.MapFrom(src => src.Metadata.ProtocolType))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Health, opt => opt.MapFrom(src => src.Health))
            .ForMember(dest => dest.Configuration, opt => opt.MapFrom(src => src.Configuration))
            .ForMember(dest => dest.Channels, opt => opt.MapFrom(src => src.Channels));

        CreateMap<LinkHealth, LinkHealthDto>()
            .ForMember(dest => dest.LastErrorMessage, opt => opt.MapFrom(src => src.ErrorMessage ?? string.Empty));

        CreateMap<LinkConfiguration, LinkConfigurationDto>();

        CreateMap<Channel, ChannelDto>()
            .ForMember(dest => dest.ChannelType, opt => opt.MapFrom(src => src.Type.Name))
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => src.Status.Name == "Active"))
            .ForMember(dest => dest.DataPoints, opt => opt.MapFrom(src => src.DataPoints));

        CreateMap<DataPoint, DataPointDto>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.Name))
            .ForMember(dest => dest.AccessMode, opt => opt.MapFrom(src => src.IsWritable ? "ReadWrite" : "ReadOnly"))
            .ForMember(dest => dest.Quality, opt => opt.MapFrom(src => src.DataQuality ?? "Unknown"))
            .ForMember(dest => dest.CurrentValue, opt => opt.MapFrom(src => src.CurrentValue))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated))
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => src.ChannelId.HasValue));

        // Reverse mappings for creation DTOs
        CreateMap<CreateLinkDto, LinkMetadata>()
            .ConstructUsing(src => new LinkMetadata(src.Name, src.Description, src.ProtocolType, "1.0.0", null));

        CreateMap<CreateLinkDto, LinkConfiguration>()
            .ConstructUsing(src => new LinkConfiguration(
                src.Configuration.ConnectionString,
                src.Configuration.Parameters,
                src.Configuration.ConnectionTimeout,
                src.Configuration.ReadTimeout, // This maps to OperationTimeout
                src.Configuration.HeartbeatInterval, // This maps to HealthCheckInterval
                10, // maxChannels default
                true, // autoReconnect default  
                src.Configuration.MaxRetries));

        CreateMap<LinkConfigurationDto, LinkConfiguration>()
            .ConstructUsing(src => new LinkConfiguration(
                src.ConnectionString,
                src.Parameters,
                src.ConnectionTimeout,
                src.ReadTimeout, // This maps to OperationTimeout
                src.HeartbeatInterval, // This maps to HealthCheckInterval
                10, // maxChannels default
                true, // autoReconnect default
                src.MaxRetries));
    }
}

/// <summary>
/// Extension methods for mapping configuration
/// </summary>
public static class MappingExtensions
{
    public static void AddCommunicationMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(CommunicationMappingProfile));
    }
}