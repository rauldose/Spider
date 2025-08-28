using Spider.Core.Application.Interfaces;
using Spider.ConnectionManagement.Application.DTOs;

namespace Spider.ConnectionManagement.Application.Commands;

public record CreateConnectionCommand(
    Guid DeviceId,
    string Name,
    string Protocol,
    string Host,
    int Port,
    int TimeoutMs = 5000,
    int RetryAttempts = 3,
    Dictionary<string, object>? ExtendedProperties = null) : ICommand<ConnectionDto>;

public record ConnectCommand(Guid ConnectionId) : ICommand<bool>;

public record DisconnectCommand(Guid ConnectionId, string? Reason = null) : ICommand<bool>;

public record UpdateConnectionParametersCommand(
    Guid ConnectionId,
    string Host,
    int Port,
    int TimeoutMs,
    int RetryAttempts,
    Dictionary<string, object>? ExtendedProperties = null) : ICommand<ConnectionDto>;

public record DeleteConnectionCommand(Guid ConnectionId) : ICommand<bool>;

public record TestConnectionCommand(
    string Protocol,
    string Host,
    int Port,
    int TimeoutMs = 5000,
    Dictionary<string, object>? ExtendedProperties = null) : ICommand<bool>;