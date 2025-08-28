using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spider.ConnectionManagement.Application.Interfaces;
using Spider.ConnectionManagement.Domain.ValueObjects;
using System.Collections.Concurrent;

namespace Spider.ConnectionManagement.Infrastructure.Services;

public class ConnectionMonitorService : BackgroundService, IConnectionMonitorService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConnectionMonitorService> _logger;
    private readonly ConcurrentDictionary<Guid, ConnectionMonitor> _activeMonitors;
    private readonly Timer _healthCheckTimer;
    private bool _isRunning;

    public ConnectionMonitorService(IServiceProvider serviceProvider, ILogger<ConnectionMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _activeMonitors = new ConcurrentDictionary<Guid, ConnectionMonitor>();
        _healthCheckTimer = new Timer(PerformHealthChecks, null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task StartMonitoringAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        if (_activeMonitors.ContainsKey(connectionId))
        {
            _logger.LogDebug("Connection {ConnectionId} is already being monitored", connectionId);
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var connectionRepository = scope.ServiceProvider.GetRequiredService<IConnectionRepository>();
            
            var connection = await connectionRepository.GetByIdAsync(connectionId, cancellationToken);
            if (connection == null)
            {
                _logger.LogWarning("Cannot monitor connection {ConnectionId} - not found", connectionId);
                return;
            }

            var monitor = new ConnectionMonitor(connection.Id, _logger);
            _activeMonitors.TryAdd(connectionId, monitor);
            
            _logger.LogInformation("Started monitoring connection {ConnectionId}", connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start monitoring connection {ConnectionId}", connectionId);
        }
    }

    public async Task StopMonitoringAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        if (_activeMonitors.TryRemove(connectionId, out var monitor))
        {
            monitor.Dispose();
            _logger.LogInformation("Stopped monitoring connection {ConnectionId}", connectionId);
        }

        await Task.CompletedTask;
    }

    public async Task<ConnectionHealth> CheckHealthAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var connectionRepository = scope.ServiceProvider.GetRequiredService<IConnectionRepository>();
            
            var connection = await connectionRepository.GetByIdAsync(connectionId, cancellationToken);
            if (connection == null)
            {
                return ConnectionHealth.Unhealthy("Connection not found");
            }

            // For now, return the stored health. In a full implementation, this would
            // perform an actual health check against the physical connection
            return connection.Health;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check health for connection {ConnectionId}", connectionId);
            return ConnectionHealth.Unhealthy($"Health check failed: {ex.Message}");
        }
    }

    public async Task StartHealthMonitoringAsync(CancellationToken cancellationToken = default)
    {
        _isRunning = true;
        _healthCheckTimer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(1)); // Check every minute
        _logger.LogInformation("Started connection health monitoring service");
        await Task.CompletedTask;
    }

    public async Task StopHealthMonitoringAsync()
    {
        _isRunning = false;
        _healthCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
        
        // Stop all individual monitors
        var monitors = _activeMonitors.Values.ToList();
        foreach (var monitor in monitors)
        {
            monitor.Dispose();
        }
        _activeMonitors.Clear();
        
        _logger.LogInformation("Stopped connection health monitoring service");
        await Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartHealthMonitoringAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested && _isRunning)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async void PerformHealthChecks(object? state)
    {
        if (!_isRunning || _activeMonitors.IsEmpty)
            return;

        var activeConnections = _activeMonitors.Keys.ToList();
        _logger.LogDebug("Performing health checks for {Count} monitored connections", activeConnections.Count);

        var tasks = activeConnections.Select(async connectionId =>
        {
            try
            {
                var health = await CheckHealthAsync(connectionId);
                
                using var scope = _serviceProvider.CreateScope();
                var connectionRepository = scope.ServiceProvider.GetRequiredService<IConnectionRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<Spider.Core.SharedKernel.Abstractions.IUnitOfWork>();
                
                var connection = await connectionRepository.GetByIdAsync(connectionId);
                if (connection != null)
                {
                    connection.UpdateHealth(health);
                    await unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for connection {ConnectionId}", connectionId);
            }
        });

        await Task.WhenAll(tasks);
    }

    public override void Dispose()
    {
        _healthCheckTimer?.Dispose();
        
        foreach (var monitor in _activeMonitors.Values)
        {
            monitor.Dispose();
        }
        _activeMonitors.Clear();
        
        base.Dispose();
    }
}

public class ConnectionMonitor : IDisposable
{
    private readonly Guid _connectionId;
    private readonly ILogger _logger;
    private bool _disposed;

    public ConnectionMonitor(Guid connectionId, ILogger logger)
    {
        _connectionId = connectionId;
        _logger = logger;
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _logger.LogDebug("Disposed connection monitor for {ConnectionId}", _connectionId);
    }
}