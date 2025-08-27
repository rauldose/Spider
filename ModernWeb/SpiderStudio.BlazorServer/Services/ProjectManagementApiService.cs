using Spider.ProjectManagement.Application.Commands;
using Spider.ProjectManagement.Application.DTOs;
using Spider.ProjectManagement.Application.Queries;
using System.Text.Json;

namespace SpiderStudio.BlazorServer.Services;

/// <summary>
/// HTTP client service for Project Management API
/// </summary>
public class ProjectManagementApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProjectManagementApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProjectManagementApiService(HttpClient httpClient, ILogger<ProjectManagementApiService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Gets all projects
    /// </summary>
    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/projects", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<IEnumerable<ProjectDto>>(json, _jsonOptions) ?? Enumerable.Empty<ProjectDto>();
            }
            
            _logger.LogError("Failed to get projects. Status: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<ProjectDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all projects");
            return Enumerable.Empty<ProjectDto>();
        }
    }

    /// <summary>
    /// Gets project by ID
    /// </summary>
    public async Task<ProjectDto?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/projects/{projectId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ProjectDto>(json, _jsonOptions);
            }
            
            _logger.LogError("Failed to get project {ProjectId}. Status: {StatusCode}", projectId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", projectId);
            return null;
        }
    }

    /// <summary>
    /// Gets projects by parent project ID
    /// </summary>
    public async Task<IEnumerable<ProjectDto>> GetProjectsByParentAsync(Guid parentProjectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/projects/parent/{parentProjectId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<IEnumerable<ProjectDto>>(json, _jsonOptions) ?? Enumerable.Empty<ProjectDto>();
            }
            
            _logger.LogError("Failed to get projects for parent {ParentProjectId}. Status: {StatusCode}", parentProjectId, response.StatusCode);
            return Enumerable.Empty<ProjectDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for parent {ParentProjectId}", parentProjectId);
            return Enumerable.Empty<ProjectDto>();
        }
    }

    /// <summary>
    /// Creates a new project
    /// </summary>
    public async Task<ProjectDto?> CreateProjectAsync(CreateProjectDto createProject, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(createProject, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/projects", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ProjectDto>(responseJson, _jsonOptions);
            }
            
            _logger.LogError("Failed to create project. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return null;
        }
    }

    /// <summary>
    /// Updates a project
    /// </summary>
    public async Task<bool> UpdateProjectAsync(Guid projectId, UpdateProjectDto updateProject, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(updateProject, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/api/projects/{projectId}", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            _logger.LogError("Failed to update project {ProjectId}. Status: {StatusCode}", projectId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", projectId);
            return false;
        }
    }

    /// <summary>
    /// Changes project status
    /// </summary>
    public async Task<bool> ChangeProjectStatusAsync(Guid projectId, ProjectStatusChangeDto statusChange, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(statusChange, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"/api/projects/{projectId}/status", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            _logger.LogError("Failed to change status for project {ProjectId}. Status: {StatusCode}", projectId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing status for project {ProjectId}", projectId);
            return false;
        }
    }

    /// <summary>
    /// Updates project configuration
    /// </summary>
    public async Task<bool> UpdateProjectConfigurationAsync(Guid projectId, ProjectConfigurationUpdateDto configUpdate, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(configUpdate, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/api/projects/{projectId}/configuration", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            _logger.LogError("Failed to update configuration for project {ProjectId}. Status: {StatusCode}", projectId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration for project {ProjectId}", projectId);
            return false;
        }
    }

    /// <summary>
    /// Gets project statistics
    /// </summary>
    public async Task<ProjectStatisticsDto?> GetProjectStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/projects/statistics", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ProjectStatisticsDto>(json, _jsonOptions);
            }
            
            _logger.LogError("Failed to get project statistics. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project statistics");
            return null;
        }
    }

    /// <summary>
    /// Checks if the Project Management API is healthy
    /// </summary>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/projects/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Project Management API health");
            return false;
        }
    }
}