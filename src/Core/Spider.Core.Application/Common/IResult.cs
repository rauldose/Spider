namespace Spider.Core.Application.Common;

/// <summary>
/// Represents the result of an operation
/// </summary>
public interface IResult
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    bool IsSuccess { get; }
    
    /// <summary>
    /// Indicates whether the operation failed
    /// </summary>
    bool IsFailure { get; }
    
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    string? Error { get; }
    
    /// <summary>
    /// Collection of error messages
    /// </summary>
    string[] Errors { get; }
}

/// <summary>
/// Represents the result of an operation with a value
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public interface IResult<out T> : IResult
{
    /// <summary>
    /// The value of the result if successful
    /// </summary>
    T? Value { get; }
}

/// <summary>
/// Represents a paged result
/// </summary>
/// <typeparam name="T">The type of items in the page</typeparam>
public interface IPagedResult<out T> : IResult<IEnumerable<T>>
{
    /// <summary>
    /// Current page number
    /// </summary>
    int Page { get; }
    
    /// <summary>
    /// Page size
    /// </summary>
    int PageSize { get; }
    
    /// <summary>
    /// Total number of items
    /// </summary>
    int TotalCount { get; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    int TotalPages { get; }
    
    /// <summary>
    /// Indicates whether there is a previous page
    /// </summary>
    bool HasPreviousPage { get; }
    
    /// <summary>
    /// Indicates whether there is a next page
    /// </summary>
    bool HasNextPage { get; }
}