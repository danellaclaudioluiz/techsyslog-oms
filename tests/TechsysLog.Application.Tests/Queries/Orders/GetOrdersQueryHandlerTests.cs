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

public class GetOrdersQueryHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersQueryHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new GetOrdersQueryHandler(_orderRepository, _mapper);
    }

    private static Order CreateTestOrder(int sequence, Guid? userId = null)
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
            OrderNumber.Generate(sequence),
            $"Test order {sequence}",
            100m * sequence,
            address,
            userId ?? Guid.NewGuid()).Value;
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            CreateTestOrder(1),
            CreateTestOrder(2),
            CreateTestOrder(3)
        };

        var query = new GetOrdersQuery { Limit = 20 };

        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(orders);

        _orderRepository.CountAsync(null, Arg.Any<CancellationToken>())
            .Returns(3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithUserIdFilter_ShouldReturnUserOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<Order>
        {
            CreateTestOrder(1, userId),
            CreateTestOrder(2, userId)
        };

        var query = new GetOrdersQuery
        {
            UserId = userId,
            Limit = 20
        };

        _orderRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(orders);

        _orderRepository.CountAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Order, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.All(o => o.UserId == userId).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldReturnOrdersWithStatus()
    {
        // Arrange
        var order1 = CreateTestOrder(1);
        order1.Confirm();

        var order2 = CreateTestOrder(2);
        order2.Confirm();

        var orders = new List<Order> { order1, order2 };

        var query = new GetOrdersQuery
        {
            Status = OrderStatus.Confirmed,
            Limit = 20
        };

        _orderRepository.GetByStatusAsync(OrderStatus.Confirmed, Arg.Any<CancellationToken>())
            .Returns(orders);

        _orderRepository.CountAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Order, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.All(o => o.Status == OrderStatus.Confirmed).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithLimit_ShouldReturnLimitedResults()
    {
        // Arrange
        var orders = Enumerable.Range(1, 5)
            .Select(i => CreateTestOrder(i))
            .ToList();

        var query = new GetOrdersQuery { Limit = 3 };

        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(orders);

        _orderRepository.CountAsync(null, Arg.Any<CancellationToken>())
            .Returns(5);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.HasMore.Should().BeTrue();
        result.Cursor.Should().NotBeNullOrEmpty();
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WithNoOrders_ShouldReturnEmptyResult()
    {
        // Arrange
        var query = new GetOrdersQuery { Limit = 20 };

        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Order>());

        _orderRepository.CountAsync(null, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.HasMore.Should().BeFalse();
        result.Cursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectPaginationInfo()
    {
        // Arrange
        var orders = Enumerable.Range(1, 25)
            .Select(i => CreateTestOrder(i))
            .ToList();

        var query = new GetOrdersQuery { Limit = 10 };

        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(orders);

        _orderRepository.CountAsync(null, Arg.Any<CancellationToken>())
            .Returns(25);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(10);
        result.Limit.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.HasMore.Should().BeTrue();
    }
}