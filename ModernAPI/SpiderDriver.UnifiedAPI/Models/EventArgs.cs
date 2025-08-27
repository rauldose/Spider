namespace SpiderDriver.UnifiedAPI.Models;

/// <summary>
/// Event arguments for connection status changes
/// </summary>
public class ConnectionStatusChangedEventArgs : EventArgs
{
    public ConnectionStatus OldStatus { get; }
    public ConnectionStatus NewStatus { get; }
    public string? Message { get; }
    public DateTime Timestamp { get; }

    public ConnectionStatusChangedEventArgs(ConnectionStatus oldStatus, ConnectionStatus newStatus, string? message = null)
    {
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Message = message;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Event arguments for data value changes
/// </summary>
public class DataValueChangedEventArgs : EventArgs
{
    public string Address { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
    public DateTime Timestamp { get; }
    public string RegisterType { get; }

    public DataValueChangedEventArgs(string address, object? oldValue, object? newValue, string registerType)
    {
        Address = address;
        OldValue = oldValue;
        NewValue = newValue;
        RegisterType = registerType;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Event arguments for driver errors
/// </summary>
public class DriverErrorEventArgs : EventArgs
{
    public string ErrorCode { get; }
    public string ErrorMessage { get; }
    public Exception? Exception { get; }
    public DateTime Timestamp { get; }
    public string? Context { get; }

    public DriverErrorEventArgs(string errorCode, string errorMessage, Exception? exception = null, string? context = null)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        Exception = exception;
        Context = context;
        Timestamp = DateTime.UtcNow;
    }
}