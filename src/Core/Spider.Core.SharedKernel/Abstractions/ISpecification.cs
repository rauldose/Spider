using System.Linq.Expressions;

namespace Spider.Core.SharedKernel.Abstractions;

/// <summary>
/// Specification pattern interface for encapsulating query logic
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// The criteria expression
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }
    
    /// <summary>
    /// Include expressions for related entities
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }
    
    /// <summary>
    /// Include expressions for string-based includes
    /// </summary>
    List<string> IncludeStrings { get; }
    
    /// <summary>
    /// Order by expression
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }
    
    /// <summary>
    /// Order by descending expression
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }
    
    /// <summary>
    /// Group by expression
    /// </summary>
    Expression<Func<T, object>>? GroupBy { get; }
    
    /// <summary>
    /// Number of items to take (for paging)
    /// </summary>
    int? Take { get; }
    
    /// <summary>
    /// Number of items to skip (for paging)
    /// </summary>
    int? Skip { get; }
    
    /// <summary>
    /// Whether paging is enabled
    /// </summary>
    bool IsPagingEnabled { get; }
}