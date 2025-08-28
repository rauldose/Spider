using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Spider.Drivers.Core.Abstractions;
using Spider.Drivers.Core.Implementations;
using Spider.Drivers.Core.Models;

namespace Spider.Drivers.Core.Testing;

/// <summary>
/// Development/Testing driver factory that includes mock and demo drivers
/// This factory should only be used in development and testing scenarios
/// </summary>
public class DevelopmentDriverFactory : IDriverFactory
{
    private readonly DriverFactory _productionFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DevelopmentDriverFactory> _logger;

    public DevelopmentDriverFactory(IServiceProvider serviceProvider, ILogger<DevelopmentDriverFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Create production factory and add development drivers
        var productionLogger = serviceProvider.GetRequiredService<ILogger<DriverFactory>>();
        _productionFactory = new DriverFactory(serviceProvider, productionLogger);
        
        // Register development/testing drivers
        RegisterDevelopmentDrivers();
    }

    /// <summary>
    /// Create a driver for the specified protocol
    /// </summary>
    public IDriver CreateDriver(string protocolType, DriverConfiguration? configuration = null)
    {
        return _productionFactory.CreateDriver(protocolType, configuration);
    }

    /// <summary>
    /// Create a driver of a specific type
    /// </summary>
    public T CreateDriver<T>(DriverConfiguration? configuration = null) where T : class, IDriver
    {
        return _productionFactory.CreateDriver<T>(configuration);
    }

    /// <summary>
    /// Get all supported protocol types (including development drivers)
    /// </summary>
    public IEnumerable<string> GetSupportedProtocols()
    {
        return _productionFactory.GetSupportedProtocols();
    }

    /// <summary>
    /// Get driver capabilities for a protocol type
    /// </summary>
    public DriverCapabilities GetProtocolCapabilities(string protocolType)
    {
        return _productionFactory.GetProtocolCapabilities(protocolType);
    }

    /// <summary>
    /// Check if a protocol is supported
    /// </summary>
    public bool IsProtocolSupported(string protocolType)
    {
        return _productionFactory.IsProtocolSupported(protocolType);
    }

    /// <summary>
    /// Register a new driver type
    /// </summary>
    public void RegisterDriver<T>(string protocolType) where T : class, IDriver
    {
        _productionFactory.RegisterDriver<T>(protocolType);
    }

    /// <summary>
    /// Unregister a driver type
    /// </summary>
    public void UnregisterDriver(string protocolType)
    {
        _productionFactory.UnregisterDriver(protocolType);
    }

    /// <summary>
    /// Register development and testing drivers
    /// </summary>
    private void RegisterDevelopmentDrivers()
    {
        // Register mock driver for testing scenarios
        _productionFactory.RegisterDriver<MockDriver>("Mock");
        _productionFactory.RegisterDriver<MockDriver>("Test");
        _productionFactory.RegisterDriver<MockDriver>("Demo");
        
        _logger.LogInformation("Registered development/testing drivers: Mock, Test, Demo");
        _logger.LogWarning("DevelopmentDriverFactory is active - this should NOT be used in production!");
    }
}