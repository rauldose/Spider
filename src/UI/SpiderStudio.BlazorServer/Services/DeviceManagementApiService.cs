using Spider.DeviceManagement.Application.Commands;
using Spider.DeviceManagement.Application.DTOs;
using Spider.DeviceManagement.Application.Queries;
using System.Text.Json;

namespace SpiderStudio.BlazorServer.Services;

/// <summary>
/// HTTP client service for Device Management API
/// </summary>
public class DeviceManagementApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DeviceManagementApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DeviceManagementApiService(HttpClient httpClient, ILogger<DeviceManagementApiService> logger)
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
    /// Creates a new device
    /// </summary>
    public async Task<Guid?> CreateDeviceAsync(CreateDeviceCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(command, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/devices", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<Guid>(responseContent, _jsonOptions);
            }
            
            _logger.LogError("Failed to create device. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating device");
            return null;
        }
    }

    /// <summary>
    /// Gets devices by project ID
    /// </summary>
    public async Task<IEnumerable<DeviceDto>> GetDevicesByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/devices/project/{projectId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<IEnumerable<DeviceDto>>(json, _jsonOptions) ?? Enumerable.Empty<DeviceDto>();
            }
            
            _logger.LogError("Failed to get devices. Status: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<DeviceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting devices for project {ProjectId}", projectId);
            return Enumerable.Empty<DeviceDto>();
        }
    }

    /// <summary>
    /// Gets available protocol types
    /// </summary>
    public async Task<IEnumerable<string>> GetProtocolTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/devices/protocols", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<IEnumerable<string>>(json, _jsonOptions) ?? Enumerable.Empty<string>();
            }
            
            _logger.LogError("Failed to get protocol types. Status: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting protocol types");
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Checks API health
    /// </summary>
    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/devices/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Device Management API health");
            return false;
        }
    }

    /// <summary>
    /// Gets a device by ID
    /// </summary>
    public async Task<DeviceDto?> GetDeviceByIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/devices/{deviceId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<DeviceDto>(json, _jsonOptions);
            }
            
            _logger.LogError("Failed to get device {DeviceId}. Status: {StatusCode}", deviceId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device {DeviceId}", deviceId);
            return null;
        }
    }
}