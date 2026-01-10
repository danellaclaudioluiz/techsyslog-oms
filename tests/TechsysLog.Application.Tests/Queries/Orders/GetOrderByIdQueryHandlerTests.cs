using AutoMapper;
using FluentAssertions;
using NSubstitute;
using TechsysLog.Application.Mappings;
using TechsysLog.Application.Queries.Orders;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Tests.Queries.Orders;

public class GetOrderByIdQueryHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new GetOrderByIdQueryHandler(_orderRepository, _mapper);
    }

    private static Order CreateTestOrder()
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
            "Test order description",
            150.50m,
            address,
            Guid.NewGuid()).Value;
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ShouldReturnOrderDto()
    {
        // Arrange
        var order = CreateTestOrder();
        var query = new GetOrderByIdQuery { OrderId = order.Id };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Description.Should().Be("Test order description");
        result.Value.Should().Be(150.50m);
        result.Status.Should().Be(OrderStatus.Pending);
        result.DeliveryAddress.Should().NotBeNull();
        result.DeliveryAddress.Street.Should().Be("Avenida Paulista");
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldReturnNull()
    {
        // Arrange
        var query = new GetOrderByIdQuery { OrderId = Guid.NewGuid() };

        _orderRepository.GetByIdAsync(query.OrderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapAddressCorrectly()
    {
        // Arrange
        var order = CreateTestOrder();
        var query = new GetOrderByIdQuery { OrderId = order.Id };

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DeliveryAddress.Cep.Should().Be("01310100");
        result.DeliveryAddress.CepFormatted.Should().Be("01310-100");
        result.DeliveryAddress.Street.Should().Be("Avenida Paulista");
        result.DeliveryAddress.Number.Should().Be("1000");
        result.DeliveryAddress.Neighborhood.Should().Be("Bela Vista");
        result.DeliveryAddress.City.Should().Be("São Paulo");
        result.DeliveryAddress.State.Should().Be("SP");
    }
}