using AutoMapper;
using FluentAssertions;
using NSubstitute;
using TechsysLog.Application.Commands.Orders;
using TechsysLog.Application.Mappings;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Tests.Commands.Orders;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly UpdateOrderStatusCommandHandler _handler;

    public UpdateOrderStatusCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new UpdateOrderStatusCommandHandler(_orderRepository, _mapper);
    }

    private static Order CreatePendingOrder()
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

        order.ClearDomainEvents();
        return order;
    }

    [Fact]
    public async Task Handle_WithValidTransition_ShouldReturnSuccessWithUpdatedOrder()
    {
        // Arrange
        var order = CreatePendingOrder();

        var command = new UpdateOrderStatusCommand
        {
            OrderId = order.Id,
            NewStatus = OrderStatus.Confirmed
        };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(OrderStatus.Confirmed);

        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            NewStatus = OrderStatus.Confirmed
        };

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
    public async Task Handle_WithInvalidTransition_ShouldReturnFailure()
    {
        // Arrange
        var order = CreatePendingOrder();

        var command = new UpdateOrderStatusCommand
        {
            OrderId = order.Id,
            NewStatus = OrderStatus.Delivered
        };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot transition");

        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSameStatus_ShouldReturnFailure()
    {
        // Arrange
        var order = CreatePendingOrder();

        var command = new UpdateOrderStatusCommand
        {
            OrderId = order.Id,
            NewStatus = OrderStatus.Pending
        };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order already has this status.");
    }

    [Fact]
    public async Task Handle_FullOrderLifecycle_ShouldSucceed()
    {
        // Arrange
        var order = CreatePendingOrder();

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act & Assert - Pending -> Confirmed
        var result1 = await _handler.Handle(
            new UpdateOrderStatusCommand { OrderId = order.Id, NewStatus = OrderStatus.Confirmed },
            CancellationToken.None);
        result1.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);

        // Confirmed -> InTransit
        var result2 = await _handler.Handle(
            new UpdateOrderStatusCommand { OrderId = order.Id, NewStatus = OrderStatus.InTransit },
            CancellationToken.None);
        result2.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.InTransit);

        // InTransit -> Delivered
        var result3 = await _handler.Handle(
            new UpdateOrderStatusCommand { OrderId = order.Id, NewStatus = OrderStatus.Delivered },
            CancellationToken.None);
        result3.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Delivered);
    }
}