namespace Spider.Drivers.Communication.Models;

/// <summary>
/// Result of a PLC communication operation
/// </summary>
/// <typeparam name="T">Type of the value</typeparam>
public sealed class OperationResult<T>
{
    public required bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? ErrorMessage { get; init; }
    public int ErrorCode { get; init; }

    /// <summary>
    /// Implicitly convert the value to boolean for easy success checks
    /// </summary>
    public static implicit operator bool(OperationResult<T> result) => result.IsSuccess;

    public static OperationResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static OperationResult<T> Failure(string message, int errorCode = -1) => new() { IsSuccess = false, ErrorMessage = message, ErrorCode = errorCode };

    /// <summary>
    /// Deconstruct for pattern matching
    /// </summary>
    public void Deconstruct(out bool isSuccess, out T? value)
    {
        isSuccess = IsSuccess;
        value = Value;
    }
}

/// <summary>
/// Result of a PLC communication operation without a value
/// </summary>
public sealed class OperationResult
{
    public required bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int ErrorCode { get; init; }

    /// <summary>
    /// Implicitly convert the result to boolean for easy success checks
    /// </summary>
    public static implicit operator bool(OperationResult result) => result.IsSuccess;

    public static OperationResult Success() => new() { IsSuccess = true };
    public static OperationResult Failure(string message, int errorCode = -1) => new() { IsSuccess = false, ErrorMessage = message, ErrorCode = errorCode };
}
