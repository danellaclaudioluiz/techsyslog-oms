using AutoMapper;
using FluentAssertions;
using NSubstitute;
using TechsysLog.Application.Mappings;
using TechsysLog.Application.Queries.Deliveries;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Tests.Queries.Deliveries;

public class GetDeliveryByOrderIdQueryHandlerTests
{
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IMapper _mapper;
    private readonly GetDeliveryByOrderIdQueryHandler _handler;

    public GetDeliveryByOrderIdQueryHandlerTests()
    {
        _deliveryRepository = Substitute.For<IDeliveryRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new GetDeliveryByOrderIdQueryHandler(_deliveryRepository, _mapper);
    }

    private static (Order order, Delivery delivery) CreateTestDelivery()
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

        var delivery = Delivery.Create(order, Guid.NewGuid()).Value;

        return (order, delivery);
    }

    [Fact]
    public async Task Handle_WithExistingDelivery_ShouldReturnDeliveryDto()
    {
        // Arrange
        var (order, delivery) = CreateTestDelivery();
        var query = new GetDeliveryByOrderIdQuery { OrderId = order.Id };

        _deliveryRepository.GetByOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(delivery);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(delivery.Id);
        result.OrderId.Should().Be(order.Id);
        result.OrderNumber.Should().Be(order.OrderNumber.Value);
        result.UserId.Should().Be(order.UserId);
        result.DeliveredBy.Should().Be(delivery.DeliveredBy);
        result.DeliveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_WithNonExistentDelivery_ShouldReturnNull()
    {
        // Arrange
        var query = new GetDeliveryByOrderIdQuery { OrderId = Guid.NewGuid() };

        _deliveryRepository.GetByOrderIdAsync(query.OrderId, Arg.Any<CancellationToken>())
            .Returns((Delivery?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetDeliveryByOrderIdQuery { OrderId = orderId };

        _deliveryRepository.GetByOrderIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Delivery?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _deliveryRepository.Received(1).GetByOrderIdAsync(orderId, Arg.Any<CancellationToken>());
    }
}