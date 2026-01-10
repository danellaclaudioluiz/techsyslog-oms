using TechsysLog.Domain.Common;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Events;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Entities;

/// <summary>
/// Represents an order aggregate root.
/// Contains business rules for status transitions and delivery validation.
/// </summary>
public sealed class Order : AggregateRoot
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        { OrderStatus.Pending, new[] { OrderStatus.Confirmed, OrderStatus.Cancelled } },
        { OrderStatus.Confirmed, new[] { OrderStatus.InTransit, OrderStatus.Cancelled } },
        { OrderStatus.InTransit, new[] { OrderStatus.Delivered } },
        { OrderStatus.Delivered, Array.Empty<OrderStatus>() },
        { OrderStatus.Cancelled, Array.Empty<OrderStatus>() }
    };

    private Order() { } // EF/MongoDB constructor

    private Order(
        OrderNumber orderNumber,
        string description,
        decimal value,
        Address deliveryAddress,
        Guid userId)
    {
        OrderNumber = orderNumber;
        Description = description;
        Value = value;
        DeliveryAddress = deliveryAddress;
        UserId = userId;
        Status = OrderStatus.Pending;
    }

    public OrderNumber OrderNumber { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Value { get; private set; }
    public Address DeliveryAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public Guid UserId { get; private set; }

    public static Result<Order> Create(
        OrderNumber orderNumber,
        string? description,
        decimal value,
        Address deliveryAddress,
        Guid userId)
    {
        if (orderNumber is null)
            return Result.Failure<Order>("Order number is required.");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Order>("Description is required.");

        if (description.Length > 500)
            return Result.Failure<Order>("Description must not exceed 500 characters.");

        if (value <= 0)
            return Result.Failure<Order>("Value must be greater than zero.");

        if (deliveryAddress is null)
            return Result.Failure<Order>("Delivery address is required.");

        if (userId == Guid.Empty)
            return Result.Failure<Order>("User ID is required.");

        var order = new Order(orderNumber, description.Trim(), value, deliveryAddress, userId);

        order.RaiseDomainEvent(new OrderCreatedEvent(
            order.Id,
            order.OrderNumber.Value,
            order.UserId,
            order.Value));

        return Result.Success(order);
    }

    public Result UpdateStatus(OrderStatus newStatus)
    {
        if (Status == newStatus)
            return Result.Failure("Order already has this status.");

        if (!CanTransitionTo(newStatus))
            return Result.Failure($"Cannot transition from {Status} to {newStatus}.");

        var oldStatus = Status;
        Status = newStatus;
        SetUpdated();

        RaiseDomainEvent(new OrderStatusChangedEvent(
            Id,
            OrderNumber.Value,
            UserId,
            oldStatus,
            newStatus));

        return Result.Success();
    }

    public Result Confirm()
    {
        return UpdateStatus(OrderStatus.Confirmed);
    }

    public Result StartDelivery()
    {
        return UpdateStatus(OrderStatus.InTransit);
    }

    public Result Cancel()
    {
        if (Status == OrderStatus.InTransit)
            return Result.Failure("Cannot cancel an order that is already in transit.");

        if (Status == OrderStatus.Delivered)
            return Result.Failure("Cannot cancel an order that was already delivered.");

        return UpdateStatus(OrderStatus.Cancelled);
    }

    public bool CanBeDelivered()
    {
        return Status == OrderStatus.InTransit;
    }

    public bool CanTransitionTo(OrderStatus newStatus)
    {
        return AllowedTransitions.TryGetValue(Status, out var allowed) 
            && allowed.Contains(newStatus);
    }

    public Result UpdateDescription(string? description)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure("Can only update description for pending orders.");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure("Description is required.");

        if (description.Length > 500)
            return Result.Failure("Description must not exceed 500 characters.");

        Description = description.Trim();
        SetUpdated();
        return Result.Success();
    }

    public Result UpdateDeliveryAddress(Address address)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure("Can only update address for pending orders.");

        if (address is null)
            return Result.Failure("Delivery address is required.");

        DeliveryAddress = address;
        SetUpdated();
        return Result.Success();
    }
}