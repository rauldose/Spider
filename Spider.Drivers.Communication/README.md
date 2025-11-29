# Spider.Drivers.Communication

A modern .NET 8 library providing a unified interface for communicating with industrial PLCs (Programmable Logic Controllers).

## Supported Protocols

- **Modbus TCP** - Standard Modbus over TCP/IP
- **Siemens S7** - S7-COMM protocol for S1200, S300, S400, S1500, S200
- **Allen-Bradley CIP** - EtherNet/IP Common Industrial Protocol

## Features

- ✅ Modern C# 12 with `required` properties and `init` setters
- ✅ Full `async/await` support with `CancellationToken`
- ✅ `IAsyncDisposable` for proper resource cleanup
- ✅ `OperationResult<T>` pattern for type-safe error handling
- ✅ Thread-safe communication via `SemaphoreSlim`
- ✅ Connection status change events
- ✅ Implicit boolean conversion for result checking

## Quick Start

### Installation

Add a reference to the `Spider.Drivers.Communication` project.

### Basic Usage

```csharp
using Spider.Drivers.Communication;
using Spider.Drivers.Communication.Models;

// Create a Modbus TCP driver
await using var driver = PlcDriverFactory.CreateModbusTcp(new ModbusTcpSettings
{
    IpAddress = "192.168.1.100",
    Port = 502,
    SlaveId = 1
});

// Connect to the PLC
var connectResult = await driver.ConnectAsync();
if (!connectResult) // Uses implicit bool conversion
{
    Console.WriteLine($"Failed: {connectResult.ErrorMessage}");
    return;
}

// Read a holding register
var result = await driver.ReadInt16Async("HR:0");
if (result.IsSuccess)
{
    Console.WriteLine($"Value: {result.Value}");
}

// Write a value
await driver.WriteInt16Async("HR:0", 42);
```

### Driver Factory Examples

```csharp
// Modbus TCP
var modbus = PlcDriverFactory.CreateModbusTcp("192.168.1.100");
var modbus2 = PlcDriverFactory.CreateModbusTcp("192.168.1.100", port: 502, slaveId: 1);

// Siemens S7
var siemens = PlcDriverFactory.CreateSiemensS7("192.168.1.200", SiemensPlcType.S1200);
var siemens2 = PlcDriverFactory.CreateSiemensS7(new SiemensS7Settings
{
    IpAddress = "192.168.1.200",
    PlcType = SiemensPlcType.S1500,
    Rack = 0,
    Slot = 1
});

// Allen-Bradley CIP
var ab = PlcDriverFactory.CreateAllenBradleyCip("192.168.1.50");
var ab2 = PlcDriverFactory.CreateAllenBradleyCip(new AllenBradleyCipSettings
{
    IpAddress = "192.168.1.50",
    Slot = 0
});
```

### Address Formats

#### Modbus TCP
- `HR:n` - Holding Register at address n
- `IR:n` - Input Register at address n  
- `CS:n` - Coil Status at address n
- `IS:n` - Input Status at address n
- `4xxxx` - Holding Register (40001-49999)
- `3xxxx` - Input Register (30001-39999)
- `1xxxx` - Discrete Input (10001-19999)
- `0xxxx` - Coil (00001-09999)

#### Siemens S7
- `DB10.DBW0` - Word at byte 0 in DB10
- `DB10.DBD0` - Double word at byte 0 in DB10
- `DB10.DBB0` - Byte at byte 0 in DB10
- `DB10.DBX0.0` - Bit 0 of byte 0 in DB10
- `MB0`, `MW0`, `MD0` - Memory byte/word/dword
- `IB0`, `IW0`, `ID0` - Input byte/word/dword
- `QB0`, `QW0`, `QD0` - Output byte/word/dword

#### Allen-Bradley CIP
- Tag names directly: `MyTag`, `Program:MainProgram.MyTag`
- Array elements: `MyArray[0]`

### Result Pattern

```csharp
// Using pattern matching
var (success, value) = await driver.ReadInt32Async("HR:0");
if (success)
{
    Console.WriteLine($"Value: {value}");
}

// Using implicit bool conversion
var result = await driver.ReadFloatAsync("HR:0");
if (result) // Same as result.IsSuccess
{
    Console.WriteLine($"Value: {result.Value}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage} (Code: {result.ErrorCode})");
}
```

### Connection Events

```csharp
driver.ConnectionStatusChanged += (sender, isConnected) =>
{
    Console.WriteLine($"Connection changed: {isConnected}");
};
```

### Supported Data Types

| Method | Data Type |
|--------|-----------|
| `ReadBoolAsync` / `WriteBoolAsync` | `bool` |
| `ReadByteAsync` / `WriteByteAsync` | `byte` |
| `ReadInt16Async` / `WriteInt16Async` | `short` |
| `ReadUInt16Async` / `WriteUInt16Async` | `ushort` |
| `ReadInt32Async` / `WriteInt32Async` | `int` |
| `ReadUInt32Async` / `WriteUInt32Async` | `uint` |
| `ReadInt64Async` / `WriteInt64Async` | `long` |
| `ReadUInt64Async` / `WriteUInt64Async` | `ulong` |
| `ReadFloatAsync` / `WriteFloatAsync` | `float` |
| `ReadDoubleAsync` / `WriteDoubleAsync` | `double` |
| `ReadStringAsync` / `WriteStringAsync` | `string` |

Array variants are also available (e.g., `ReadInt16ArrayAsync`).

## Sample Application

Run the sample console application to test drivers interactively:

```bash
cd Spider.Drivers.Communication.Sample
dotnet run
```

## License

This project is part of the Spider SCADA system.
