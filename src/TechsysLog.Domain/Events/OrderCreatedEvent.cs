using TechsysLog.Domain.Common;

namespace TechsysLog.Domain.Events;

/// <summary>
/// Raised when a new order is created.
/// Triggers notifications to relevant users.
/// </summary>
public sealed record OrderCreatedEvent : DomainEventBase
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public Guid UserId { get; }
    public decimal Value { get; }

    public OrderCreatedEvent(Guid orderId, string orderNumber, Guid userId, decimal value)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        UserId = userId;
        Value = value;
    }
}