namespace TechsysLog.Domain.Common;

/// <summary>
/// Base class for aggregate roots.
/// Aggregates are consistency boundaries that publish domain events.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<DomainEventBase> _domainEvents = new();

    /// <summary>
    /// Domain events raised by this aggregate.
    /// Events are dispatched after the aggregate is persisted.
    /// </summary>
    public IReadOnlyCollection<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(DomainEventBase domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}