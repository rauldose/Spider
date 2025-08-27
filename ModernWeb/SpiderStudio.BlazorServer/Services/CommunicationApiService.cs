using System.Text;
using System.Text.Json;

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

    public async Task<bool> CreateLinkAsync(CreateLinkRequest request)
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

    public async Task<bool> CreateChannelAsync(CreateChannelRequest request)
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
            var response = await _httpClient.GetAsync("api/communication/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking communication service health");
            return false;
        }
    }
}

// DTOs
public record LinkDto(
    Guid Id,
    string Name,
    string Description,
    string Protocol,
    string Status,
    DateTime CreatedAt,
    DateTime? LastConnectedAt,
    bool IsHealthy);

public record CreateLinkRequest(
    string Name,
    string Description,
    string Protocol,
    Dictionary<string, string> Configuration);

public record AttachDriverRequest(
    string DriverType,
    Dictionary<string, string> Configuration);

public record LinkHealthDto(
    bool IsHealthy,
    string Status,
    double SuccessRate,
    double AverageResponseTime,
    DateTime LastCheckAt);

public record ChannelDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    Guid LinkId,
    int DataPointCount);

public record CreateChannelRequest(
    string Name,
    string Description,
    string Type,
    Guid LinkId);

public record CommunicationStatisticsDto(
    int TotalLinks,
    int ConnectedLinks,
    int FailedLinks,
    int TotalChannels,
    int TotalDataPoints,
    double OverallHealthPercentage);