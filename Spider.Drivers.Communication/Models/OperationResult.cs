namespace Spider.Drivers.Communication.Models;

/// <summary>
/// Result of a PLC communication operation
/// </summary>
/// <typeparam name="T">Type of the value</typeparam>
public class OperationResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? ErrorMessage { get; set; }
    public int ErrorCode { get; set; }

    public static OperationResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static OperationResult<T> Failure(string message, int errorCode = -1) => new() { IsSuccess = false, ErrorMessage = message, ErrorCode = errorCode };
}

/// <summary>
/// Result of a PLC communication operation without a value
/// </summary>
public class OperationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int ErrorCode { get; set; }

    public static OperationResult Success() => new() { IsSuccess = true };
    public static OperationResult Failure(string message, int errorCode = -1) => new() { IsSuccess = false, ErrorMessage = message, ErrorCode = errorCode };
}
