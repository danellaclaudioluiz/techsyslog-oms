using MediatR;
using TechsysLog.Domain.Common;

namespace TechsysLog.Application.Common;

/// <summary>
/// Marker interface for commands that return Result.
/// Commands represent intentions to change state.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Marker interface for commands that return Result with value.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Handler interface for commands that return Result.
/// </summary>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Handler interface for commands that return Result with value.
/// </summary>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}