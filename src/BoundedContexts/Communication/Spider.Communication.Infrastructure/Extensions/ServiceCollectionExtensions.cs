using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spider.Communication.Application.Interfaces;
using Spider.Communication.Infrastructure.Persistence;
using Spider.Communication.Infrastructure.Repositories;
using Spider.Core.SharedKernel.Abstractions;
using Spider.Core.Application.Interfaces;
using Spider.Core.Application.Common;
using FluentValidation;
using Spider.Communication.Application.Validators;
using Spider.Communication.Application.Mappings;

namespace Spider.Communication.Infrastructure.Extensions;

/// <summary>
/// Dependency injection extensions for Communication Infrastructure
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommunicationInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<CommunicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                // Use In-Memory database for development/testing
                options.UseInMemoryDatabase("CommunicationDB");
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        // Add repositories
        services.AddScoped<ILinkRepository, LinkRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IDataPointRepository, DataPointRepository>();

        // Add UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add validation
        services.AddValidatorsFromAssemblyContaining<CreateLinkCommandValidator>();

        // Add AutoMapper
        services.AddCommunicationMappings();

        return services;
    }

    public static IServiceCollection AddCommunicationApplication(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            typeof(Spider.Communication.Application.Commands.CreateLinkCommand).Assembly));

        return services;
    }
}

/// <summary>
/// Unit of Work implementation for Communication context
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CommunicationDbContext _context;

    public UnitOfWork(CommunicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}