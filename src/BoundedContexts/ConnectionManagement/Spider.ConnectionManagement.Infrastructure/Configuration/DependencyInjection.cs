using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spider.ConnectionManagement.Application.Interfaces;
using Spider.ConnectionManagement.Infrastructure.Drivers;
using Spider.ConnectionManagement.Infrastructure.Persistence;
using Spider.ConnectionManagement.Infrastructure.Repositories;
using Spider.ConnectionManagement.Infrastructure.Services;
using Spider.Core.SharedKernel.Abstractions;

namespace Spider.ConnectionManagement.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddConnectionManagementInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<ConnectionManagementDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
        else
        {
            // Fallback to in-memory database for development/testing
            services.AddDbContext<ConnectionManagementDbContext>(options =>
                options.UseInMemoryDatabase("ConnectionManagement"));
        }

        // Repository pattern
        services.AddScoped<IConnectionRepository, ConnectionRepository>();
        services.AddScoped<IUnitOfWork, ConnectionUnitOfWork>();

        // Protocol drivers
        services.AddSingleton<IProtocolDriverFactory, ProtocolDriverFactory>();

        // Background services
        services.AddSingleton<IConnectionMonitorService, ConnectionMonitorService>();
        services.AddHostedService<ConnectionMonitorService>(provider => 
            (ConnectionMonitorService)provider.GetRequiredService<IConnectionMonitorService>());

        return services;
    }

    public static async Task<IServiceProvider> EnsureConnectionManagementDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConnectionManagementDbContext>();
        
        try
        {
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetService<Microsoft.Extensions.Logging.ILogger<ConnectionManagementDbContext>>();
            logger?.LogError(ex, "Failed to ensure ConnectionManagement database");
            throw;
        }

        return serviceProvider;
    }
}