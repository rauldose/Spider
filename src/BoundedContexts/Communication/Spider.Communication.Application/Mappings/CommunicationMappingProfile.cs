using AutoMapper;
using Spider.Communication.Application.DTOs;
using Spider.Communication.Domain.Entities;
using Spider.Communication.Domain.ValueObjects;

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
            .ForMember(dest => dest.LastErrorMessage, opt => opt.MapFrom(src => src.LastErrorMessage ?? string.Empty));

        CreateMap<LinkConfiguration, LinkConfigurationDto>();

        CreateMap<Channel, ChannelDto>()
            .ForMember(dest => dest.ChannelType, opt => opt.MapFrom(src => src.ChannelType.ToString()))
            .ForMember(dest => dest.DataPoints, opt => opt.MapFrom(src => src.DataPoints));

        CreateMap<DataPoint, DataPointDto>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address.Value))
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.ToString()))
            .ForMember(dest => dest.AccessMode, opt => opt.MapFrom(src => src.AccessMode.ToString()))
            .ForMember(dest => dest.Quality, opt => opt.MapFrom(src => src.CurrentValue != null ? src.CurrentValue.Quality.ToString() : "Unknown"))
            .ForMember(dest => dest.CurrentValue, opt => opt.MapFrom(src => src.CurrentValue != null ? src.CurrentValue.Value : null))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.CurrentValue != null ? src.CurrentValue.Timestamp : (DateTime?)null));

        // Reverse mappings for creation DTOs
        CreateMap<CreateLinkDto, LinkMetadata>()
            .ConstructUsing(src => new LinkMetadata(src.Name, src.Description, src.ProtocolType));

        CreateMap<CreateLinkDto, LinkConfiguration>()
            .ConstructUsing(src => new LinkConfiguration(
                src.Configuration.ConnectionString,
                src.Configuration.Parameters,
                src.Configuration.ConnectionTimeout,
                src.Configuration.ReadTimeout,
                src.Configuration.MaxRetries,
                src.Configuration.EnableHeartbeat,
                src.Configuration.HeartbeatInterval));

        CreateMap<LinkConfigurationDto, LinkConfiguration>()
            .ConstructUsing(src => new LinkConfiguration(
                src.ConnectionString,
                src.Parameters,
                src.ConnectionTimeout,
                src.ReadTimeout,
                src.MaxRetries,
                src.EnableHeartbeat,
                src.HeartbeatInterval));

        CreateMap<CreateChannelDto, Channel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.LinkId, opt => opt.MapFrom(src => src.LinkId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<CreateDataPointDto, DataPoint>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.ChannelId, opt => opt.MapFrom(src => src.ChannelId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
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