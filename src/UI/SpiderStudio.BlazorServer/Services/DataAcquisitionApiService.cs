using Spider.DataAcquisition.Application.Commands;
using Spider.DataAcquisition.Application.DTOs;
using System.Text.Json;

namespace SpiderStudio.BlazorServer.Services;

/// <summary>
/// HTTP client service for Data Acquisition API
/// </summary>
public class DataAcquisitionApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DataAcquisitionApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DataAcquisitionApiService(HttpClient httpClient, ILogger<DataAcquisitionApiService> logger)
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
    /// Creates a new data point
    /// </summary>
    public async Task<Guid?> CreateDataPointAsync(CreateDataPointCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(command, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/datapoints", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<Guid>(responseContent, _jsonOptions);
            }
            
            _logger.LogError("Failed to create data point. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating data point");
            return null;
        }
    }

    /// <summary>
    /// Gets data points for a device
    /// </summary>
    public async Task<IEnumerable<DataPointDto>> GetDataPointsByDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/datapoints/device/{deviceId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<IEnumerable<DataPointDto>>(json, _jsonOptions) ?? Enumerable.Empty<DataPointDto>();
            }
            
            _logger.LogError("Failed to get data points. Status: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<DataPointDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data points for device {DeviceId}", deviceId);
            return Enumerable.Empty<DataPointDto>();
        }
    }

    /// <summary>
    /// Updates a data point value
    /// </summary>
    public async Task<bool> UpdateDataPointValueAsync(Guid dataPointId, object value, string quality = "Good", CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UpdateDataPointValueCommand(dataPointId, value, quality);
            var json = JsonSerializer.Serialize(command, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/api/datapoints/{dataPointId}/value", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            _logger.LogError("Failed to update data point value. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating data point value for {DataPointId}", dataPointId);
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
            // Try the standard health endpoint first
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            if (response.IsSuccessStatusCode)
                return true;
                
            // Fallback to API-specific health endpoint
            response = await _httpClient.GetAsync("/api/datapoints/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Data Acquisition API is unavailable: {Message}", ex.Message);
            return false;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Data Acquisition API health check timed out");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking Data Acquisition API health");
            return false;
        }
    }
}