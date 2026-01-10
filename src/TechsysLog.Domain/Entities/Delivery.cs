using TechsysLog.Domain.Common;
using TechsysLog.Domain.Events;

namespace TechsysLog.Domain.Entities;

/// <summary>
/// Represents a delivery record for an order.
/// Created when an order is successfully delivered.
/// </summary>
public sealed class Delivery : AggregateRoot
{
    private Delivery() { } // EF/MongoDB constructor

    private Delivery(Guid orderId, string orderNumber, Guid userId, Guid deliveredBy)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        UserId = userId;
        DeliveredBy = deliveredBy;
        DeliveredAt = DateTime.UtcNow;
    }

    public Guid OrderId { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Guid DeliveredBy { get; private set; }
    public DateTime DeliveredAt { get; private set; }

    /// <summary>
    /// Creates a new delivery record.
    /// The order must be validated as deliverable before calling this method.
    /// </summary>
    public static Result<Delivery> Create(
        Order order,
        Guid deliveredBy)
    {
        if (order is null)
            return Result.Failure<Delivery>("Order is required.");

        if (!order.CanBeDelivered())
            return Result.Failure<Delivery>("Order cannot be delivered. Status must be InTransit.");

        if (deliveredBy == Guid.Empty)
            return Result.Failure<Delivery>("DeliveredBy is required.");

        var delivery = new Delivery(
            order.Id,
            order.OrderNumber.Value,
            order.UserId,
            deliveredBy);

        delivery.RaiseDomainEvent(new OrderDeliveredEvent(
            order.Id,
            order.OrderNumber.Value,
            order.UserId,
            delivery.Id,
            delivery.DeliveredAt));

        return Result.Success(delivery);
    }
}