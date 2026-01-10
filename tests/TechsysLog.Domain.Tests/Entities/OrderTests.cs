using FluentAssertions;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Events;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Tests.Entities;

public class OrderTests
{
    private static OrderNumber ValidOrderNumber => OrderNumber.Generate(1);
    private static Cep ValidCep => Cep.Create("01310100").Value;
    private static Address ValidAddress => Address.Create(
        ValidCep,
        "Avenida Paulista",
        "1000",
        "Bela Vista",
        "São Paulo",
        "SP").Value;
    private static Guid ValidUserId => Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var userId = ValidUserId;

        // Act
        var result = Order.Create(
            ValidOrderNumber,
            "Test order description",
            100.50m,
            ValidAddress,
            userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrderNumber.Should().Be(ValidOrderNumber);
        result.Value.Description.Should().Be("Test order description");
        result.Value.Value.Should().Be(100.50m);
        result.Value.DeliveryAddress.Should().Be(ValidAddress);
        result.Value.UserId.Should().Be(userId);
        result.Value.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Create_ShouldRaiseOrderCreatedEvent()
    {
        // Act
        var result = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId);

        // Assert
        result.Value.DomainEvents.Should().ContainSingle();
        var domainEvent = result.Value.DomainEvents.First();
        domainEvent.Should().BeOfType<OrderCreatedEvent>();

        var orderCreatedEvent = (OrderCreatedEvent)domainEvent;
        orderCreatedEvent.OrderId.Should().Be(result.Value.Id);
        orderCreatedEvent.OrderNumber.Should().Be(result.Value.OrderNumber.Value);
        orderCreatedEvent.UserId.Should().Be(result.Value.UserId);
        orderCreatedEvent.Value.Should().Be(100m);
    }

    [Fact]
    public void Create_WithNullOrderNumber_ShouldReturnFailure()
    {
        // Act
        var result = Order.Create(
            null!,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order number is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ShouldReturnFailure(string? description)
    {
        // Act
        var result = Order.Create(
            ValidOrderNumber,
            description,
            100m,
            ValidAddress,
            ValidUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Description is required.");
    }

    [Fact]
    public void Create_WithDescriptionExceeding500Characters_ShouldReturnFailure()
    {
        // Arrange
        var description = new string('a', 501);

        // Act
        var result = Order.Create(
            ValidOrderNumber,
            description,
            100m,
            ValidAddress,
            ValidUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Description must not exceed 500 characters.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_WithInvalidValue_ShouldReturnFailure(decimal value)
    {
        // Act
        var result = Order.Create(
            ValidOrderNumber,
            "Test order",
            value,
            ValidAddress,
            ValidUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Value must be greater than zero.");
    }

    [Fact]
    public void Create_WithNullAddress_ShouldReturnFailure()
    {
        // Act
        var result = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            null!,
            ValidUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Delivery address is required.");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        // Act
        var result = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User ID is required.");
    }

    [Fact]
    public void Confirm_FromPending_ShouldReturnSuccess()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;
        order.ClearDomainEvents();

        // Act
        var result = order.Confirm();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.DomainEvents.Should().ContainSingle();
        order.DomainEvents.First().Should().BeOfType<OrderStatusChangedEvent>();
    }

    [Fact]
    public void StartDelivery_FromConfirmed_ShouldReturnSuccess()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;
        order.Confirm();
        order.ClearDomainEvents();

        // Act
        var result = order.StartDelivery();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.InTransit);
    }

    [Fact]
    public void UpdateStatus_ToDelivered_FromInTransit_ShouldReturnSuccess()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;
        order.Confirm();
        order.StartDelivery();
        order.ClearDomainEvents();

        // Act
        var result = order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void UpdateStatus_WithSameStatus_ShouldReturnFailure()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;

        // Act
        var result = order.UpdateStatus(OrderStatus.Pending);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order already has this status.");
    }

    [Fact]
    public void UpdateStatus_WithInvalidTransition_ShouldReturnFailure()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;

        // Act
        var result = order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot transition from Pending to Delivered.");
    }

    [Fact]
    public void UpdateStatus_ShouldRaiseOrderStatusChangedEvent()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;
        order.ClearDomainEvents();

        // Act
        order.Confirm();

        // Assert
        var domainEvent = order.DomainEvents.First() as OrderStatusChangedEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.OldStatus.Should().Be(OrderStatus.Pending);
        domainEvent.NewStatus.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Cancel_FromPending_ShouldReturnSuccess()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;

        // Act
        var result = order.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromConfirmed_ShouldReturnSuccess()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;
        order.Confirm();

        // Act
        var result = order.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromInTransit_ShouldReturnFailure()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;
        order.Confirm();
        order.StartDelivery();

        // Act
        var result = order.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot cancel an order that is already in transit.");
    }

    [Fact]
    public void Cancel_FromDelivered_ShouldReturnFailure()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;
        order.Confirm();
        order.StartDelivery();
        order.UpdateStatus(OrderStatus.Delivered);

        // Act
        var result = order.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot cancel an order that was already delivered.");
    }

    [Fact]
    public void CanBeDelivered_WhenInTransit_ShouldReturnTrue()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;
        order.Confirm();
        order.StartDelivery();

        // Act
        var result = order.CanBeDelivered();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void CanBeDelivered_WhenNotInTransit_ShouldReturnFalse(OrderStatus status)
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;

        if (status == OrderStatus.Confirmed)
            order.Confirm();
        else if (status == OrderStatus.Delivered)
        {
            order.Confirm();
            order.StartDelivery();
            order.UpdateStatus(OrderStatus.Delivered);
        }
        else if (status == OrderStatus.Cancelled)
            order.Cancel();

        // Act
        var result = order.CanBeDelivered();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateDescription_WhenPending_ShouldReturnSuccess()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;

        // Act
        var result = order.UpdateDescription("Updated description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Description.Should().Be("Updated description");
    }

    [Fact]
    public void UpdateDescription_WhenNotPending_ShouldReturnFailure()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;
        order.Confirm();

        // Act
        var result = order.UpdateDescription("Updated description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Can only update description for pending orders.");
    }

    [Fact]
    public void UpdateDeliveryAddress_WhenPending_ShouldReturnSuccess()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;

        var newAddress = Address.Create(
            Cep.Create("04538132").Value,
            "Rua Funchal",
            "500",
            "Vila Olímpia",
            "São Paulo",
            "SP").Value;

        // Act
        var result = order.UpdateDeliveryAddress(newAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.DeliveryAddress.Should().Be(newAddress);
    }

    [Fact]
    public void UpdateDeliveryAddress_WhenNotPending_ShouldReturnFailure()
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;
        order.Confirm();

        var newAddress = Address.Create(
            Cep.Create("04538132").Value,
            "Rua Funchal",
            "500",
            "Vila Olímpia",
            "São Paulo",
            "SP").Value;

        // Act
        var result = order.UpdateDeliveryAddress(newAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Can only update address for pending orders.");
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.InTransit, false)]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.InTransit, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.InTransit, OrderStatus.Delivered, true)]
    [InlineData(OrderStatus.InTransit, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending, false)]
    public void CanTransitionTo_ShouldReturnExpectedResult(
        OrderStatus currentStatus,
        OrderStatus newStatus,
        bool expected)
    {
        // Arrange
        var order = Order.Create(
            ValidOrderNumber,
            "Test order",
            100m,
            ValidAddress,
            ValidUserId).Value;

        // Move to current status
        if (currentStatus == OrderStatus.Confirmed)
            order.Confirm();
        else if (currentStatus == OrderStatus.InTransit)
        {
            order.Confirm();
            order.StartDelivery();
        }
        else if (currentStatus == OrderStatus.Delivered)
        {
            order.Confirm();
            order.StartDelivery();
            order.UpdateStatus(OrderStatus.Delivered);
        }
        else if (currentStatus == OrderStatus.Cancelled)
            order.Cancel();

        // Act
        var result = order.CanTransitionTo(newStatus);

        // Assert
        result.Should().Be(expected);
    }
}