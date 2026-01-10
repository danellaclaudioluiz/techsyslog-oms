using MediatR;

namespace TechsysLog.Application.Common;

/// <summary>
/// Marker interface for queries.
/// Queries represent intentions to read state without side effects.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Handler interface for queries.
/// </summary>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}