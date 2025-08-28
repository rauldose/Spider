namespace Spider.Core.Application.Common;

/// <summary>
/// Implementation of IResult
/// </summary>
public class Result : IResult
{
    protected Result(bool isSuccess, string? error, string[] errors)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? Array.Empty<string>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string[] Errors { get; }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new(true, null, Array.Empty<string>());

    /// <summary>
    /// Creates a failure result with an error message
    /// </summary>
    public static Result Failure(string error) => new(false, error, new[] { error });

    /// <summary>
    /// Creates a failure result with multiple error messages
    /// </summary>
    public static Result Failure(string[] errors) => new(false, errors.FirstOrDefault(), errors);

    /// <summary>
    /// Creates a failure result with an error message and multiple errors
    /// </summary>
    public static Result Failure(string error, string[] errors) => new(false, error, errors);
}

/// <summary>
/// Implementation of IResult with a value
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public class Result<T> : Result, IResult<T>
{
    private readonly T? _value;

    protected Result(T? value, bool isSuccess, string? error, string[] errors) 
        : base(isSuccess, error, errors)
    {
        _value = value;
    }

    public T? Value => IsSuccess ? _value : default;

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    public static Result<T> Success(T value) => new(value, true, null, Array.Empty<string>());

    /// <summary>
    /// Creates a failure result with an error message
    /// </summary>
    public static new Result<T> Failure(string error) => new(default, false, error, new[] { error });

    /// <summary>
    /// Creates a failure result with multiple error messages
    /// </summary>
    public static new Result<T> Failure(string[] errors) => new(default, false, errors.FirstOrDefault(), errors);

    /// <summary>
    /// Creates a failure result with an error message and multiple errors
    /// </summary>
    public static new Result<T> Failure(string error, string[] errors) => new(default, false, error, errors);

    /// <summary>
    /// Implicit conversion from value to Result
    /// </summary>
    /// <param name="value">The value to convert</param>
    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Implementation of IPagedResult with items
/// </summary>
/// <typeparam name="T">The type of items in the page</typeparam>
public class PagedResult<T> : Result<IEnumerable<T>>, IPagedResult<T>
{
    protected PagedResult(
        IEnumerable<T>? items, 
        int page, 
        int pageSize, 
        int totalCount,
        bool isSuccess, 
        string? error, 
        string[] errors) 
        : base(items, isSuccess, error, errors)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        HasPreviousPage = page > 1;
        HasNextPage = page < TotalPages;
    }

    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }

    /// <summary>
    /// Creates a successful paged result
    /// </summary>
    public static PagedResult<T> Success(IEnumerable<T> items, int page, int pageSize, int totalCount)
        => new(items, page, pageSize, totalCount, true, null, Array.Empty<string>());

    /// <summary>
    /// Creates a failure paged result
    /// </summary>
    public static new PagedResult<T> Failure(string error)
        => new(null, 0, 0, 0, false, error, new[] { error });

    /// <summary>
    /// Creates a failure paged result with multiple errors
    /// </summary>
    public static new PagedResult<T> Failure(string[] errors)
        => new(null, 0, 0, 0, false, errors.FirstOrDefault(), errors);
}