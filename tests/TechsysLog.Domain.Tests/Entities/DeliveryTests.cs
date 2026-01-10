using FluentAssertions;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Events;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Tests.Entities;

public class DeliveryTests
{
    private static Order CreateOrderInTransit()
    {
        var order = Order.Create(
            OrderNumber.Generate(1),
            "Test order",
            100m,
            Address.Create(
                Cep.Create("01310100").Value,
                "Avenida Paulista",
                "1000",
                "Bela Vista",
                "São Paulo",
                "SP").Value,
            Guid.NewGuid()).Value;

        order.Confirm();
        order.StartDelivery();
        order.ClearDomainEvents();

        return order;
    }

    private static Order CreatePendingOrder()
    {
        return Order.Create(
            OrderNumber.Generate(1),
            "Test order",
            100m,
            Address.Create(
                Cep.Create("01310100").Value,
                "Avenida Paulista",
                "1000",
                "Bela Vista",
                "São Paulo",
                "SP").Value,
            Guid.NewGuid()).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var order = CreateOrderInTransit();
        var deliveredBy = Guid.NewGuid();

        // Act
        var result = Delivery.Create(order, deliveredBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(order.Id);
        result.Value.OrderNumber.Should().Be(order.OrderNumber.Value);
        result.Value.UserId.Should().Be(order.UserId);
        result.Value.DeliveredBy.Should().Be(deliveredBy);
        result.Value.DeliveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldRaiseOrderDeliveredEvent()
    {
        // Arrange
        var order = CreateOrderInTransit();
        var deliveredBy = Guid.NewGuid();

        // Act
        var result = Delivery.Create(order, deliveredBy);

        // Assert
        result.Value.DomainEvents.Should().ContainSingle();
        var domainEvent = result.Value.DomainEvents.First();
        domainEvent.Should().BeOfType<OrderDeliveredEvent>();

        var orderDeliveredEvent = (OrderDeliveredEvent)domainEvent;
        orderDeliveredEvent.OrderId.Should().Be(order.Id);
        orderDeliveredEvent.OrderNumber.Should().Be(order.OrderNumber.Value);
        orderDeliveredEvent.UserId.Should().Be(order.UserId);
        orderDeliveredEvent.DeliveryId.Should().Be(result.Value.Id);
    }

    [Fact]
    public void Create_WithNullOrder_ShouldReturnFailure()
    {
        // Arrange
        var deliveredBy = Guid.NewGuid();

        // Act
        var result = Delivery.Create(null!, deliveredBy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order is required.");
    }

    [Fact]
    public void Create_WithOrderNotInTransit_ShouldReturnFailure()
    {
        // Arrange
        var order = CreatePendingOrder();
        var deliveredBy = Guid.NewGuid();

        // Act
        var result = Delivery.Create(order, deliveredBy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order cannot be delivered. Status must be InTransit.");
    }

    [Fact]
    public void Create_WithEmptyDeliveredBy_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateOrderInTransit();

        // Act
        var result = Delivery.Create(order, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("DeliveredBy is required.");
    }

    [Fact]
    public void Create_WithConfirmedOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = CreatePendingOrder();
        order.Confirm();
        var deliveredBy = Guid.NewGuid();

        // Act
        var result = Delivery.Create(order, deliveredBy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order cannot be delivered. Status must be InTransit.");
    }

    [Fact]
    public void Create_WithDeliveredOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateOrderInTransit();
        order.UpdateStatus(Enums.OrderStatus.Delivered);
        var deliveredBy = Guid.NewGuid();

        // Act
        var result = Delivery.Create(order, deliveredBy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order cannot be delivered. Status must be InTransit.");
    }

    [Fact]
    public void Create_WithCancelledOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = CreatePendingOrder();
        order.Cancel();
        var deliveredBy = Guid.NewGuid();

        // Act
        var result = Delivery.Create(order, deliveredBy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order cannot be delivered. Status must be InTransit.");
    }
}