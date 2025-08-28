namespace Spider.Core.Application.Interfaces;

/// <summary>
/// Interface for the current user context
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    string? UserId { get; }
    
    /// <summary>
    /// Gets the current user's name
    /// </summary>
    string? UserName { get; }
    
    /// <summary>
    /// Gets the current user's email
    /// </summary>
    string? Email { get; }
    
    /// <summary>
    /// Checks if the current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    bool IsInRole(string role);
    
    /// <summary>
    /// Gets all roles for the current user
    /// </summary>
    IEnumerable<string> GetRoles();
}