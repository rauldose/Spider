using Spider.Core.SharedKernel.Base;

namespace Spider.ConnectionManagement.Domain.Enumerations;

public class ConnectionStatus : Enumeration
{
    public static readonly ConnectionStatus Disconnected = new(1, nameof(Disconnected));
    public static readonly ConnectionStatus Connecting = new(2, nameof(Connecting));
    public static readonly ConnectionStatus Connected = new(3, nameof(Connected));
    public static readonly ConnectionStatus Reconnecting = new(4, nameof(Reconnecting));
    public static readonly ConnectionStatus Failed = new(5, nameof(Failed));
    public static readonly ConnectionStatus TimedOut = new(6, nameof(TimedOut));

    private ConnectionStatus(int id, string name) : base(id, name) { }

    public bool IsConnected => this == Connected;
    public bool IsDisconnected => this == Disconnected || this == Failed || this == TimedOut;
    public bool IsTransitioning => this == Connecting || this == Reconnecting;
}