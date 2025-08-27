using Microsoft.Extensions.Diagnostics.HealthChecks;
using Spider.ConnectionManagement.Application.Interfaces;

namespace Spider.ConnectionManagement.API.Health;

public class ConnectionManagementHealthCheck : IHealthCheck
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly ILogger<ConnectionManagementHealthCheck> _logger;

    public ConnectionManagementHealthCheck(
        IConnectionRepository connectionRepository,
        ILogger<ConnectionManagementHealthCheck> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if we can query the repository
            var connectionCount = await _connectionRepository.CountAsync(null, cancellationToken);
            var unhealthyConnections = await _connectionRepository.GetUnhealthyAsync(cancellationToken);
            var unhealthyCount = unhealthyConnections.Count();

            var data = new Dictionary<string, object>
            {
                ["total_connections"] = connectionCount,
                ["unhealthy_connections"] = unhealthyCount,
                ["timestamp"] = DateTime.UtcNow
            };

            if (unhealthyCount > connectionCount * 0.5) // More than 50% unhealthy
            {
                return HealthCheckResult.Degraded(
                    $"High number of unhealthy connections: {unhealthyCount}/{connectionCount}",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Connection Management is healthy. {connectionCount} total connections, {unhealthyCount} unhealthy",
                data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return HealthCheckResult.Unhealthy(
                "Connection Management health check failed",
                ex);
        }
    }
}