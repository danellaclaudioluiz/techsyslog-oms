using FluentAssertions;
using NSubstitute;
using TechsysLog.Application.Commands.Orders;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Tests.Commands.Orders;

public class CancelOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _handler = new CancelOrderCommandHandler(_orderRepository);
    }

    private static Order CreateOrder(OrderStatus targetStatus = OrderStatus.Pending)
    {
        var cep = Cep.Create("01310100").Value;
        var address = Address.Create(
            cep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP").Value;

        var order = Order.Create(
            OrderNumber.Generate(1),
            "Test order",
            100m,
            address,
            Guid.NewGuid()).Value;

        if (targetStatus == OrderStatus.Confirmed)
            order.Confirm();
        else if (targetStatus == OrderStatus.InTransit)
        {
            order.Confirm();
            order.StartDelivery();
        }
        else if (targetStatus == OrderStatus.Delivered)
        {
            order.Confirm();
            order.StartDelivery();
            order.UpdateStatus(OrderStatus.Delivered);
        }

        order.ClearDomainEvents();
        return order;
    }

    [Fact]
    public async Task Handle_WithPendingOrder_ShouldReturnSuccess()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Pending);

        var command = new CancelOrderCommand { OrderId = order.Id };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);

        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithConfirmedOrder_ShouldReturnSuccess()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Confirmed);

        var command = new CancelOrderCommand { OrderId = order.Id };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldReturnFailure()
    {
        // Arrange
        var command = new CancelOrderCommand { OrderId = Guid.NewGuid() };

        _orderRepository.GetByIdAsync(command.OrderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order not found.");

        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInTransitOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.InTransit);

        var command = new CancelOrderCommand { OrderId = order.Id };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("in transit");

        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDeliveredOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Delivered);

        var command = new CancelOrderCommand { OrderId = order.Id };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("delivered");

        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }
}