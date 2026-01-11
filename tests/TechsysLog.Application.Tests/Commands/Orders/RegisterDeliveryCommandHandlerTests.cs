using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using TechsysLog.Application.Commands.Deliveries;
using TechsysLog.Application.Mappings;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Tests.Commands.Deliveries;

public class RegisterDeliveryCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly RegisterDeliveryCommandHandler _handler;

    public RegisterDeliveryCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _deliveryRepository = Substitute.For<IDeliveryRepository>();
        _mediator = Substitute.For<IMediator>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new RegisterDeliveryCommandHandler(
            _orderRepository,
            _deliveryRepository,
            _mapper,
            _mediator);
    }

    private static Order CreateOrderInTransit()
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

        order.Confirm();
        order.StartDelivery();
        order.ClearDomainEvents();

        return order;
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

        return Order.Create(
            OrderNumber.Generate(1),
            "Test order",
            100m,
            address,
            Guid.NewGuid()).Value;
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithDeliveryDto()
    {
        // Arrange
        var order = CreateOrderInTransit();
        var deliveredBy = Guid.NewGuid();

        var command = new RegisterDeliveryCommand
        {
            OrderId = order.Id,
            DeliveredBy = deliveredBy
        };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        _deliveryRepository.OrderHasDeliveryAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        _deliveryRepository.AddAsync(Arg.Any<Delivery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Delivery>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.OrderId.Should().Be(order.Id);
        result.Value.DeliveredBy.Should().Be(deliveredBy);
        result.Value.DeliveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        await _deliveryRepository.Received(1).AddAsync(Arg.Any<Delivery>(), Arg.Any<CancellationToken>());
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateOrderStatusToDelivered()
    {
        // Arrange
        var order = CreateOrderInTransit();
        var deliveredBy = Guid.NewGuid();

        var command = new RegisterDeliveryCommand
        {
            OrderId = order.Id,
            DeliveredBy = deliveredBy
        };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        _deliveryRepository.OrderHasDeliveryAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        _deliveryRepository.AddAsync(Arg.Any<Delivery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Delivery>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterDeliveryCommand
        {
            OrderId = Guid.NewGuid(),
            DeliveredBy = Guid.NewGuid()
        };

        _orderRepository.GetByIdAsync(command.OrderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order not found.");

        await _deliveryRepository.DidNotReceive().AddAsync(Arg.Any<Delivery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingDelivery_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateOrderInTransit();

        var command = new RegisterDeliveryCommand
        {
            OrderId = order.Id,
            DeliveredBy = Guid.NewGuid()
        };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        _deliveryRepository.OrderHasDeliveryAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order already has a delivery record.");

        await _deliveryRepository.DidNotReceive().AddAsync(Arg.Any<Delivery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPendingOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = CreatePendingOrder();

        var command = new RegisterDeliveryCommand
        {
            OrderId = order.Id,
            DeliveredBy = Guid.NewGuid()
        };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        _deliveryRepository.OrderHasDeliveryAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("InTransit");

        await _deliveryRepository.DidNotReceive().AddAsync(Arg.Any<Delivery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyDeliveredBy_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateOrderInTransit();

        var command = new RegisterDeliveryCommand
        {
            OrderId = order.Id,
            DeliveredBy = Guid.Empty
        };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        _deliveryRepository.OrderHasDeliveryAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("DeliveredBy");
    }

    [Fact]
    public async Task Handle_ShouldPublishOrderDeliveredEvent()
    {
        // Arrange
        var order = CreateOrderInTransit();
        var deliveredBy = Guid.NewGuid();

        var command = new RegisterDeliveryCommand
        {
            OrderId = order.Id,
            DeliveredBy = deliveredBy
        };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        _deliveryRepository.OrderHasDeliveryAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        _deliveryRepository.AddAsync(Arg.Any<Delivery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Delivery>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }
}