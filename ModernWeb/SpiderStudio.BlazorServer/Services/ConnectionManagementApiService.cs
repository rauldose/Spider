using Spider.ConnectionManagement.Application.Commands;
using Spider.ConnectionManagement.Application.DTOs;
using Spider.ConnectionManagement.Application.Queries;
using System.Text.Json;

namespace SpiderStudio.BlazorServer.Services;

/// <summary>
/// HTTP client service for Connection Management API
/// </summary>
public class ConnectionManagementApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConnectionManagementApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConnectionManagementApiService(HttpClient httpClient, ILogger<ConnectionManagementApiService> logger)
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
    /// Gets all connections
    /// </summary>
    public async Task<IEnumerable<ConnectionDto>> GetAllConnectionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/connections", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<IEnumerable<ConnectionDto>>(json, _jsonOptions) ?? Enumerable.Empty<ConnectionDto>();
            }
            
            _logger.LogError("Failed to get connections. Status: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<ConnectionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all connections");
            return Enumerable.Empty<ConnectionDto>();
        }
    }

    /// <summary>
    /// Gets connections by device ID
    /// </summary>
    public async Task<IEnumerable<ConnectionDto>> GetConnectionsByDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/connections/device/{deviceId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<IEnumerable<ConnectionDto>>(json, _jsonOptions) ?? Enumerable.Empty<ConnectionDto>();
            }
            
            _logger.LogError("Failed to get connections for device {DeviceId}. Status: {StatusCode}", deviceId, response.StatusCode);
            return Enumerable.Empty<ConnectionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connections for device {DeviceId}", deviceId);
            return Enumerable.Empty<ConnectionDto>();
        }
    }

    /// <summary>
    /// Gets connection statistics
    /// </summary>
    public async Task<ConnectionStatisticsDto?> GetConnectionStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/connections/statistics", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ConnectionStatisticsDto>(json, _jsonOptions);
            }
            
            _logger.LogError("Failed to get connection statistics. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection statistics");
            return null;
        }
    }

    /// <summary>
    /// Gets available protocols
    /// </summary>
    public async Task<IEnumerable<string>> GetAvailableProtocolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/connections/protocols", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<IEnumerable<string>>(json, _jsonOptions) ?? Enumerable.Empty<string>();
            }
            
            _logger.LogError("Failed to get available protocols. Status: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available protocols");
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Creates a new connection
    /// </summary>
    public async Task<ConnectionDto?> CreateConnectionAsync(CreateConnectionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/connections", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ConnectionDto>(responseContent, _jsonOptions);
            }
            
            _logger.LogError("Failed to create connection. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating connection");
            return null;
        }
    }

    /// <summary>
    /// Connects a connection
    /// </summary>
    public async Task<bool> ConnectAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"/api/connections/{connectionId}/connect", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);
                return result.GetProperty("success").GetBoolean();
            }
            
            _logger.LogError("Failed to connect connection {ConnectionId}. Status: {StatusCode}", connectionId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting connection {ConnectionId}", connectionId);
            return false;
        }
    }

    /// <summary>
    /// Disconnects a connection
    /// </summary>
    public async Task<bool> DisconnectAsync(Guid connectionId, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestObj = new { reason };
            var json = JsonSerializer.Serialize(requestObj, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"/api/connections/{connectionId}/disconnect", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);
                return result.GetProperty("success").GetBoolean();
            }
            
            _logger.LogError("Failed to disconnect connection {ConnectionId}. Status: {StatusCode}", connectionId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting connection {ConnectionId}", connectionId);
            return false;
        }
    }

    /// <summary>
    /// Tests connection parameters
    /// </summary>
    public async Task<bool> TestConnectionAsync(string protocol, string host, int port, int timeoutMs = 5000, 
        Dictionary<string, object>? extendedProperties = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var testRequest = new
            {
                protocol,
                host,
                port,
                timeoutMs,
                extendedProperties
            };
            
            var json = JsonSerializer.Serialize(testRequest, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/connections/test", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);
                return result.GetProperty("success").GetBoolean();
            }
            
            _logger.LogError("Failed to test connection. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection");
            return false;
        }
    }

    /// <summary>
    /// Gets connection health
    /// </summary>
    public async Task<ConnectionHealthDto?> GetConnectionHealthAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/connections/{connectionId}/health", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<ConnectionHealthDto>(json, _jsonOptions);
            }
            
            _logger.LogError("Failed to get connection health for {ConnectionId}. Status: {StatusCode}", connectionId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection health for {ConnectionId}", connectionId);
            return null;
        }
    }

    /// <summary>
    /// Deletes a connection
    /// </summary>
    public async Task<bool> DeleteConnectionAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/connections/{connectionId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting connection {ConnectionId}", connectionId);
            return false;
        }
    }

    /// <summary>
    /// Checks API health
    /// </summary>
    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/connections/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Connection Management API health");
            return false;
        }
    }
}