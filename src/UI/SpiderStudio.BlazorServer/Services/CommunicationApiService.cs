using System.Text;
using System.Text.Json;
using Spider.Communication.Application.DTOs;

namespace SpiderStudio.BlazorServer.Services;

public class CommunicationApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CommunicationApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CommunicationApiService(HttpClient httpClient, ILogger<CommunicationApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    // Link Management
    public async Task<IEnumerable<LinkDto>> GetLinksAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<LinkDto>>("api/links", _jsonOptions);
            return response ?? new List<LinkDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting links");
            return new List<LinkDto>();
        }
    }

    public async Task<LinkDto?> GetLinkByIdAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<LinkDto>($"api/links/{id}", _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting link {LinkId}", id);
            return null;
        }
    }

    public async Task<bool> CreateLinkAsync(CreateLinkDto request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/links", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating link");
            return false;
        }
    }

    public async Task<bool> ConnectLinkAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/links/{id}/connect", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting link {LinkId}", id);
            return false;
        }
    }

    public async Task<bool> DisconnectLinkAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/links/{id}/disconnect", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting link {LinkId}", id);
            return false;
        }
    }

    public async Task<bool> AttachDriverToLinkAsync(Guid linkId, AttachDriverRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"api/links/{linkId}/attach-driver", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attaching driver to link {LinkId}", linkId);
            return false;
        }
    }

    public async Task<LinkHealthDto?> GetLinkHealthAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<LinkHealthDto>($"api/links/{id}/health", _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting link health {LinkId}", id);
            return null;
        }
    }

    // Channel Management
    public async Task<IEnumerable<ChannelDto>> GetChannelsByLinkAsync(Guid linkId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<ChannelDto>>($"api/channels/link/{linkId}", _jsonOptions);
            return response ?? new List<ChannelDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channels for link {LinkId}", linkId);
            return new List<ChannelDto>();
        }
    }

    public async Task<bool> CreateChannelAsync(CreateChannelDto request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/channels", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating channel");
            return false;
        }
    }

    // Communication Statistics
    public async Task<CommunicationStatisticsDto?> GetStatisticsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CommunicationStatisticsDto>("api/communication/statistics", _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting communication statistics");
            return null;
        }
    }

    public async Task<IEnumerable<string>> GetProtocolTypesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<string>>("api/communication/protocols", _jsonOptions);
            return response ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting protocol types");
            return new List<string>();
        }
    }

    // Health Check
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            // Try the standard health endpoint first
            var response = await _httpClient.GetAsync("/health");
            if (response.IsSuccessStatusCode)
                return true;
                
            // Fallback to API-specific health endpoint
            response = await _httpClient.GetAsync("api/communication/health");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Communication API is unavailable: {Message}", ex.Message);
            return false;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Communication API health check timed out");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking communication service health");
            return false;
        }
    }
}

// Request DTOs for methods that don't have corresponding DTOs in Application layer
public record AttachDriverRequest(
    string DriverType,
    Dictionary<string, object> Configuration);

// Form models for binding (since DTOs have init-only properties)
public class CreateLinkFormModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProtocolType { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    public CreateLinkDto ToDto()
    {
        return new CreateLinkDto
        {
            Name = this.Name,
            Description = this.Description,
            ProtocolType = this.ProtocolType,
            Configuration = new LinkConfigurationDto
            {
                Parameters = this.Configuration
            }
        };
    }
}