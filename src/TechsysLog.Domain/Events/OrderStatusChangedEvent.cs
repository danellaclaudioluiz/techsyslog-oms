using TechsysLog.Domain.Common;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Domain.Events;

/// <summary>
/// Raised when an order status changes.
/// Triggers real-time updates to the dashboard.
/// </summary>
public sealed record OrderStatusChangedEvent : DomainEventBase
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public Guid UserId { get; }
    public OrderStatus OldStatus { get; }
    public OrderStatus NewStatus { get; }

    public OrderStatusChangedEvent(
        Guid orderId,
        string orderNumber,
        Guid userId,
        OrderStatus oldStatus,
        OrderStatus newStatus)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        UserId = userId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}