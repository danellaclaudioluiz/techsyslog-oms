using AutoMapper;
using FluentAssertions;
using NSubstitute;
using TechsysLog.Application.Commands.Orders;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Mappings;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Tests.Commands.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICepService _cepService;
    private readonly IMapper _mapper;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _cepService = Substitute.For<ICepService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateOrderCommandHandler(
            _orderRepository,
            _cepService,
            _mapper);
    }

    private static Address CreateTestAddress()
    {
        var cep = Cep.Create("01310100").Value;
        return Address.Create(
            cep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP").Value;
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithOrderDto()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order description",
            Value = 150.50m,
            Cep = "01310100",
            Number = "1000",
            Complement = "Apto 101",
            UserId = Guid.NewGuid()
        };

        var address = CreateTestAddress();

        _cepService.GetAddressByCepAsync(Arg.Any<Cep>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(address));

        _orderRepository.GetDailyOrderCountAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(0);

        _orderRepository.AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Order>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Description.Should().Be("Test order description");
        result.Value.Value.Should().Be(150.50m);
        result.Value.Status.Should().Be(OrderStatus.Pending);
        result.Value.UserId.Should().Be(command.UserId);
        result.Value.OrderNumber.Should().MatchRegex(@"^ORD-\d{8}-00001$");

        await _orderRepository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCep_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = "123",
            Number = "1000",
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("CEP");
    }

    [Fact]
    public async Task Handle_WhenCepServiceFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = "01310100",
            Number = "1000",
            UserId = Guid.NewGuid()
        };

        _cepService.GetAddressByCepAsync(Arg.Any<Cep>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Address>("CEP not found."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("CEP not found.");

        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGenerateSequentialOrderNumbers()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = "01310100",
            Number = "1000",
            UserId = Guid.NewGuid()
        };

        var address = CreateTestAddress();

        _cepService.GetAddressByCepAsync(Arg.Any<Cep>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(address));

        _orderRepository.GetDailyOrderCountAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(5);

        _orderRepository.AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Order>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrderNumber.Should().MatchRegex(@"^ORD-\d{8}-00006$");
    }

    [Fact]
    public async Task Handle_WithZeroValue_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 0m,
            Cep = "01310100",
            Number = "1000",
            UserId = Guid.NewGuid()
        };

        var address = CreateTestAddress();

        _cepService.GetAddressByCepAsync(Arg.Any<Cep>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(address));

        _orderRepository.GetDailyOrderCountAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Value");
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = "01310100",
            Number = "1000",
            UserId = Guid.Empty
        };

        var address = CreateTestAddress();

        _cepService.GetAddressByCepAsync(Arg.Any<Cep>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(address));

        _orderRepository.GetDailyOrderCountAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("User ID");
    }
}