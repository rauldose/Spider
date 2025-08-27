using Spider.Core.SharedKernel.Abstractions;
using Spider.ConnectionManagement.Domain.Entities;
using Spider.ConnectionManagement.Domain.ValueObjects;

namespace Spider.ConnectionManagement.Application.Interfaces;

public interface IConnectionRepository : IRepository<Connection, Guid>
{
    Task<IEnumerable<Connection>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Connection>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Connection>> GetUnhealthyAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByDeviceIdAndNameAsync(Guid deviceId, string name, CancellationToken cancellationToken = default);
}

public interface IProtocolDriverFactory
{
    IProtocolDriver CreateDriver(string protocolType);
    IEnumerable<string> GetSupportedProtocols();
}

public interface IProtocolDriver
{
    string ProtocolType { get; }
    Task<bool> TestConnectionAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default);
    Task<IConnection> ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken = default);
}

public interface IConnection : IDisposable
{
    string Id { get; }
    bool IsConnected { get; }
    ConnectionHealth Health { get; }
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    event EventHandler<ConnectionHealthChangedEventArgs>? HealthChanged;
}

public class ConnectionHealthChangedEventArgs : EventArgs
{
    public ConnectionHealth Health { get; }
    public string? Error { get; }

    public ConnectionHealthChangedEventArgs(ConnectionHealth health, string? error = null)
    {
        Health = health;
        Error = error;
    }
}

public interface IConnectionMonitorService
{
    Task StartMonitoringAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task StopMonitoringAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<ConnectionHealth> CheckHealthAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task StartHealthMonitoringAsync(CancellationToken cancellationToken = default);
    Task StopHealthMonitoringAsync();
}