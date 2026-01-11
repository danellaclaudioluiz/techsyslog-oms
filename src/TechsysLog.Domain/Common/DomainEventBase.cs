using MediatR;

namespace TechsysLog.Domain.Common;

/// <summary>
/// Base class for all domain events.
/// Implements MediatR INotification for event dispatching.
/// </summary>
public abstract record DomainEventBase : INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}