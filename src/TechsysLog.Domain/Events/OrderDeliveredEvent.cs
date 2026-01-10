using TechsysLog.Domain.Common;

namespace TechsysLog.Domain.Events;

/// <summary>
/// Raised when an order is delivered.
/// Triggers final notification to the customer.
/// </summary>
public sealed record OrderDeliveredEvent : DomainEventBase
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public Guid UserId { get; }
    public Guid DeliveryId { get; }
    public DateTime DeliveredAt { get; }

    public OrderDeliveredEvent(
        Guid orderId,
        string orderNumber,
        Guid userId,
        Guid deliveryId,
        DateTime deliveredAt)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        UserId = userId;
        DeliveryId = deliveryId;
        DeliveredAt = deliveredAt;
    }
}