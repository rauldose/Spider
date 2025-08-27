using Cdy.Spider;
using SpiderDriver.UnifiedAPI.Interfaces;
using SpiderDriver.UnifiedAPI.Adapters;

namespace SpiderDriver.UnifiedAPI.Services;

/// <summary>
/// Factory service for creating unified drivers from both legacy and modern implementations
/// Enables seamless integration of existing Spider drivers with the new unified interface
/// </summary>
public class UnifiedDriverFactory
{
    private readonly IDriverFactory? _legacyDriverFactory;
    private readonly Dictionary<string, Func<IUnifiedDriver>> _modernDriverCreators = new();
    private readonly Dictionary<string, string> _protocolDescriptions = new();

    public UnifiedDriverFactory(IDriverFactory? legacyDriverFactory = null)
    {
        _legacyDriverFactory = legacyDriverFactory;
        InitializeProtocolDescriptions();
        RegisterModernDriverCreators();
    }

    /// <summary>
    /// Create a unified driver from a legacy Spider driver
    /// </summary>
    public IUnifiedDriver? CreateFromLegacyDriver(string protocolType)
    {
        if (_legacyDriverFactory == null)
            return null;

        try
        {
            var legacyDriver = _legacyDriverFactory.GetDevelopInstance(protocolType);
            if (legacyDriver == null)
                return null;

            return new LegacyDriverAdapter(legacyDriver);
        }
        catch (Exception)
        {
            // Log error in production
            return null;
        }
    }

    /// <summary>
    /// Create a modern unified driver
    /// </summary>
    public IUnifiedDriver? CreateModernDriver(string protocolType)
    {
        if (_modernDriverCreators.TryGetValue(protocolType.ToLower(), out var creator))
        {
            return creator();
        }
        return null;
    }

    /// <summary>
    /// Create a unified driver (tries modern first, then legacy)
    /// </summary>
    public IUnifiedDriver? CreateUnifiedDriver(string protocolType)
    {
        // Try modern implementation first
        var modernDriver = CreateModernDriver(protocolType);
        if (modernDriver != null)
            return modernDriver;

        // Fallback to legacy implementation
        return CreateFromLegacyDriver(protocolType);
    }

    /// <summary>
    /// Get all available protocol types
    /// </summary>
    public IEnumerable<string> GetAvailableProtocols()
    {
        var protocols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Add modern protocols
        foreach (var protocol in _modernDriverCreators.Keys)
        {
            protocols.Add(protocol);
        }
        
        // Add legacy protocols if available
        if (_legacyDriverFactory != null)
        {
            try
            {
                // This would require extending the legacy interface to expose available types
                // For now, we'll add known legacy protocols
                var knownLegacyProtocols = new[]
                {
                    "Modbus", "OPC UA", "MQTT", "TcpClient", "UdpClient", "SerialPort",
                    "OmronFins", "Siemens", "AllenBradley", "Melsec", "SystemDriver"
                };
                
                foreach (var protocol in knownLegacyProtocols)
                {
                    protocols.Add(protocol);
                }
            }
            catch
            {
                // Ignore errors when querying legacy factory
            }
        }
        
        return protocols.OrderBy(p => p);
    }

    /// <summary>
    /// Get protocol description
    /// </summary>
    public string GetProtocolDescription(string protocolType)
    {
        return _protocolDescriptions.TryGetValue(protocolType.ToLower(), out var description) 
            ? description 
            : $"{protocolType} Protocol Driver";
    }

    /// <summary>
    /// Register a modern driver creator
    /// </summary>
    public void RegisterModernDriver<T>(string protocolType, Func<T> creator) where T : IUnifiedDriver
    {
        _modernDriverCreators[protocolType.ToLower()] = () => creator();
    }

    /// <summary>
    /// Check if a protocol has a modern implementation
    /// </summary>
    public bool HasModernImplementation(string protocolType)
    {
        return _modernDriverCreators.ContainsKey(protocolType.ToLower());
    }

    /// <summary>
    /// Check if a protocol has a legacy implementation
    /// </summary>
    public bool HasLegacyImplementation(string protocolType)
    {
        if (_legacyDriverFactory == null)
            return false;

        try
        {
            var driver = _legacyDriverFactory.GetDevelopInstance(protocolType);
            return driver != null;
        }
        catch
        {
            return false;
        }
    }

    private void InitializeProtocolDescriptions()
    {
        _protocolDescriptions.AddRange(new Dictionary<string, string>
        {
            { "modbus", "Modbus TCP/RTU/ASCII industrial communication protocol" },
            { "opcua", "OPC Unified Architecture - modern industrial automation protocol" },
            { "mqtt", "MQTT - lightweight messaging protocol for IoT" },
            { "coap", "Constrained Application Protocol for IoT devices" },
            { "tcpclient", "Generic TCP client for custom protocols" },
            { "udpclient", "Generic UDP client for custom protocols" },
            { "serialport", "Serial port communication (RS232/RS485)" },
            { "omronfins", "Omron FINS protocol for Omron PLCs" },
            { "siemens", "Siemens S7 protocol for Siemens PLCs" },
            { "allenbradley", "Allen-Bradley CIP protocol for AB PLCs" },
            { "melsec", "Mitsubishi MELSEC protocol for Mitsubishi PLCs" },
            { "systemdriver", "System driver for local system monitoring" },
            { "linkdriver", "Link driver for inter-Spider communication" },
            { "calculatedriver", "Calculate driver for computed values" },
            { "customdriver", "Custom driver with user-defined C# code" }
        });
    }

    private void RegisterModernDriverCreators()
    {
        // Register demo drivers and any modern implementations
        // Note: We'll reference the demo driver by string to avoid circular dependency
        _modernDriverCreators["demo-modbus"] = () => CreateDemoModbusDriver();
        
        // Additional modern drivers would be registered here as they're implemented
        // RegisterModernDriver("modern-opcua", () => new ModernOpcUaDriver());
        // RegisterModernDriver("modern-mqtt", () => new ModernMqttDriver());
    }

    private IUnifiedDriver CreateDemoModbusDriver()
    {
        // Use reflection to create the demo driver to avoid assembly dependency
        var assembly = System.Reflection.Assembly.LoadFrom("SpiderStudio.BlazorServer.dll");
        var type = assembly.GetType("SpiderStudio.BlazorServer.Drivers.DemoModbusDriver");
        return (IUnifiedDriver)Activator.CreateInstance(type)!;
    }
}

/// <summary>
/// Extension methods for dictionary initialization
/// </summary>
public static class DictionaryExtensions
{
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> items) where TKey : notnull
    {
        foreach (var item in items)
        {
            dictionary[item.Key] = item.Value;
        }
    }
}