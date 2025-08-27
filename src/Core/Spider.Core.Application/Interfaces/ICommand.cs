using MediatR;

namespace Spider.Core.Application.Interfaces;

/// <summary>
/// Base interface for all commands
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Base interface for commands that return a result
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Base interface for all queries
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}