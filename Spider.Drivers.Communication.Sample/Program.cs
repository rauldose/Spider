using Spider.Drivers.Communication;
using Spider.Drivers.Communication.Interfaces;
using Spider.Drivers.Communication.Models;

namespace Spider.Drivers.Communication.Sample;

/// <summary>
/// Sample program demonstrating the usage of PLC communication drivers
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       Spider.Drivers.Communication Sample Application        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Display menu
        while (true)
        {
            Console.WriteLine("Select a driver to test:");
            Console.WriteLine("  1. Modbus TCP");
            Console.WriteLine("  2. Siemens S7");
            Console.WriteLine("  3. Allen-Bradley CIP");
            Console.WriteLine("  4. Exit");
            Console.Write("\nChoice: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await TestModbusTcpAsync();
                    break;
                case "2":
                    await TestSiemensS7Async();
                    break;
                case "3":
                    await TestAllenBradleyCipAsync();
                    break;
                case "4":
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.\n");
                    break;
            }
        }
    }

    private static async Task TestModbusTcpAsync()
    {
        Console.WriteLine("\n--- Modbus TCP Driver Test ---\n");

        Console.Write("Enter PLC IP address (default: 127.0.0.1): ");
        var ipAddress = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ipAddress)) ipAddress = "127.0.0.1";

        Console.Write($"Enter port (default: {PlcDriverFactory.DefaultModbusTcpPort}): ");
        var portInput = Console.ReadLine();
        var port = string.IsNullOrWhiteSpace(portInput) ? PlcDriverFactory.DefaultModbusTcpPort : int.Parse(portInput);

        Console.Write("Enter slave ID (default: 1): ");
        var slaveIdInput = Console.ReadLine();
        var slaveId = string.IsNullOrWhiteSpace(slaveIdInput) ? (byte)1 : byte.Parse(slaveIdInput);

        // Create driver using factory with modern settings
        await using var driver = PlcDriverFactory.CreateModbusTcp(new ModbusTcpSettings
        {
            IpAddress = ipAddress,
            Port = port,
            SlaveId = slaveId,
            ConnectTimeout = 5000,
            ReceiveTimeout = 3000
        });

        await TestDriverOperationsAsync(driver, "HR:0", "CS:0");
    }

    private static async Task TestSiemensS7Async()
    {
        Console.WriteLine("\n--- Siemens S7 Driver Test ---\n");

        Console.Write("Enter PLC IP address (default: 127.0.0.1): ");
        var ipAddress = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ipAddress)) ipAddress = "127.0.0.1";

        Console.WriteLine("Select PLC type:");
        Console.WriteLine("  0. S1200");
        Console.WriteLine("  1. S300");
        Console.WriteLine("  2. S400");
        Console.WriteLine("  3. S1500");
        Console.WriteLine("  4. S200Smart");
        Console.WriteLine("  5. S200");
        Console.Write("Choice (default: 0): ");
        var plcTypeInput = Console.ReadLine();
        var plcType = string.IsNullOrWhiteSpace(plcTypeInput) ? SiemensPlcType.S1200 : (SiemensPlcType)int.Parse(plcTypeInput);

        Console.Write("Enter rack number (default: 0): ");
        var rackInput = Console.ReadLine();
        var rack = string.IsNullOrWhiteSpace(rackInput) ? (byte)0 : byte.Parse(rackInput);

        Console.Write("Enter slot number (default: 0): ");
        var slotInput = Console.ReadLine();
        var slot = string.IsNullOrWhiteSpace(slotInput) ? (byte)0 : byte.Parse(slotInput);

        // Create driver using factory with modern settings
        await using var driver = PlcDriverFactory.CreateSiemensS7(new SiemensS7Settings
        {
            IpAddress = ipAddress,
            Port = PlcDriverFactory.DefaultSiemensS7Port,
            PlcType = plcType,
            Rack = rack,
            Slot = slot,
            ConnectTimeout = 5000,
            ReceiveTimeout = 3000
        });

        await TestDriverOperationsAsync(driver, "DB1.DBW0", "DB1.DBX0.0");
    }

    private static async Task TestAllenBradleyCipAsync()
    {
        Console.WriteLine("\n--- Allen-Bradley CIP Driver Test ---\n");

        Console.Write("Enter PLC IP address (default: 127.0.0.1): ");
        var ipAddress = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ipAddress)) ipAddress = "127.0.0.1";

        Console.Write("Enter processor slot (default: 0): ");
        var slotInput = Console.ReadLine();
        var slot = string.IsNullOrWhiteSpace(slotInput) ? (byte)0 : byte.Parse(slotInput);

        // Create driver using factory with modern settings
        await using var driver = PlcDriverFactory.CreateAllenBradleyCip(new AllenBradleyCipSettings
        {
            IpAddress = ipAddress,
            Port = PlcDriverFactory.DefaultAllenBradleyCipPort,
            Slot = slot,
            ConnectTimeout = 5000,
            ReceiveTimeout = 3000
        });

        await TestDriverOperationsAsync(driver, "MyTag", "MyBoolTag");
    }

    private static async Task TestDriverOperationsAsync(IPlcDriver driver, string intAddress, string boolAddress)
    {
        Console.WriteLine($"\nDriver Type: {driver.DriverTypeName}");
        Console.WriteLine($"Connecting...\n");

        // Subscribe to connection status changes
        driver.ConnectionStatusChanged += (sender, isConnected) =>
        {
            Console.WriteLine($"[Event] Connection status changed: {(isConnected ? "Connected" : "Disconnected")}");
        };

        // Try to connect
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var connectResult = await driver.ConnectAsync(cts.Token);

        if (!connectResult.IsSuccess)
        {
            Console.WriteLine($"❌ Failed to connect: {connectResult.ErrorMessage}");
            Console.WriteLine("\nNote: This sample requires a real PLC or simulator to be running.");
            Console.WriteLine("You can use a Modbus simulator like 'ModRSsim2' for testing.\n");
            return;
        }

        Console.WriteLine("✅ Connected successfully!\n");

        // Test read operations
        Console.WriteLine("Testing read operations...\n");

        // Read Int16
        Console.WriteLine($"Reading Int16 from '{intAddress}'...");
        var int16Result = await driver.ReadInt16Async(intAddress, cts.Token);
        PrintResult("Int16", int16Result);

        // Read Bool
        Console.WriteLine($"Reading Bool from '{boolAddress}'...");
        var boolResult = await driver.ReadBoolAsync(boolAddress, cts.Token);
        PrintResult("Bool", boolResult);

        // Read Float (if supported by address format)
        Console.WriteLine($"Reading Float from '{intAddress}'...");
        var floatResult = await driver.ReadFloatAsync(intAddress, cts.Token);
        PrintResult("Float", floatResult);

        // Test write operations
        Console.WriteLine("\nTesting write operations...\n");

        Console.Write($"Enter a value to write to '{intAddress}' (Int16, or press Enter to skip): ");
        var writeValueInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(writeValueInput) && short.TryParse(writeValueInput, out var writeValue))
        {
            Console.WriteLine($"Writing {writeValue} to '{intAddress}'...");
            var writeResult = await driver.WriteInt16Async(intAddress, writeValue, cts.Token);
            if (writeResult.IsSuccess)
            {
                Console.WriteLine("✅ Write successful!");
                
                // Read back to verify
                var verifyResult = await driver.ReadInt16Async(intAddress, cts.Token);
                if (verifyResult.IsSuccess)
                {
                    Console.WriteLine($"✅ Verified: Value is now {verifyResult.Value}");
                }
            }
            else
            {
                Console.WriteLine($"❌ Write failed: {writeResult.ErrorMessage}");
            }
        }

        // Disconnect
        Console.WriteLine("\nDisconnecting...");
        var disconnectResult = await driver.DisconnectAsync(cts.Token);
        Console.WriteLine(disconnectResult.IsSuccess ? "✅ Disconnected successfully!" : $"❌ Disconnect failed: {disconnectResult.ErrorMessage}");
        Console.WriteLine();
    }

    private static void PrintResult<T>(string dataType, OperationResult<T> result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine($"  ✅ {dataType}: {result.Value}");
        }
        else
        {
            Console.WriteLine($"  ❌ {dataType}: {result.ErrorMessage} (Code: {result.ErrorCode})");
        }
    }
}
