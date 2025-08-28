using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spider.Drivers.Core.Abstractions;
using Spider.Drivers.Core.Implementations;

namespace Spider.Drivers.Core.Extensions;

/// <summary>
/// Service collection extensions for registering driver services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register the complete driver infrastructure with DI container
    /// </summary>
    public static IServiceCollection AddDriverInfrastructure(this IServiceCollection services)
    {
        // Register driver abstractions
        services.TryAddSingleton<IDriverFactory, DriverFactory>();
        services.TryAddSingleton<IDriverRepository, DriverRepository>();
        services.TryAddSingleton<IDriverManager, DriverManager>();

        // Register individual driver implementations
        services.AddTransient<ModbusDriver>();
        services.AddTransient<OpcUaDriver>();
        services.AddTransient<MqttDriver>();
        services.AddTransient<EthernetIpDriver>();
        services.AddTransient<SiemensDriver>();
        services.AddTransient<OmronDriver>();
        services.AddTransient<MitsubishiDriver>();

        return services;
    }

    /// <summary>
    /// Register a custom driver implementation
    /// </summary>
    public static IServiceCollection AddDriver<TDriver>(this IServiceCollection services)
        where TDriver : class, IDriver
    {
        services.AddTransient<TDriver>();
        return services;
    }

    /// <summary>
    /// Register driver infrastructure with custom implementations
    /// </summary>
    public static IServiceCollection AddDriverInfrastructure<TFactory, TRepository, TManager>(this IServiceCollection services)
        where TFactory : class, IDriverFactory
        where TRepository : class, IDriverRepository  
        where TManager : class, IDriverManager
    {
        services.TryAddSingleton<IDriverFactory, TFactory>();
        services.TryAddSingleton<IDriverRepository, TRepository>();
        services.TryAddSingleton<IDriverManager, TManager>();

        return services;
    }
}