using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechsysLog.API.Controllers;
using TechsysLog.API.Models;
using TechsysLog.Application.Commands.Orders;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Queries.Orders;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Enums;

namespace TechsysLog.API.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OrdersController _controller;
    private readonly Guid _userId;

    public OrdersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new OrdersController(_mediatorMock.Object);
        _userId = Guid.NewGuid();

        // Setup user context
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _userId.ToString()),
            new(ClaimTypes.Email, "operator@test.com"),
            new(ClaimTypes.Role, "Operator")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Description = "Test Order",
            Value = 100.00m,
            Cep = "01310100",
            Number = "100"
        };

        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-20260110-ABC123",
            Description = request.Description,
            Value = request.Value,
            Status = OrderStatus.Pending,
            UserId = _userId,
            CreatedAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(orderDto));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<ApiResponse<OrderDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Description = "",
            Value = -100.00m,
            Cep = "invalid",
            Number = "100"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<OrderDto>("Description is required."));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedOrders()
    {
        // Arrange
        var orders = new List<OrderDto>
        {
            new() { Id = Guid.NewGuid(), OrderNumber = "ORD-001", Status = OrderStatus.Pending },
            new() { Id = Guid.NewGuid(), OrderNumber = "ORD-002", Status = OrderStatus.Confirmed }
        };

        var pagedResult = PagedResult<OrderDto>.Create(orders, null, false, 20, 2);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(null, null, null, 20, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PagedResult<OrderDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            Status = OrderStatus.Pending,
            UserId = _userId
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.GetById(orderId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<OrderDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task GetById_WithNonExistentOrder_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.GetById(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateStatus_WithValidStatus_ShouldReturnUpdatedOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest { Status = "Confirmed" };

        var orderDto = new OrderDto
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            Status = OrderStatus.Confirmed
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(orderDto));

        // Act
        var result = await _controller.UpdateStatus(orderId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<OrderDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest { Status = "InvalidStatus" };

        // Act
        var result = await _controller.UpdateStatus(orderId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Cancel_WithExistingOrder_ShouldReturnSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Cancel(orderId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Cancel_WithNonExistentOrder_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Order not found."));

        // Act
        var result = await _controller.Cancel(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}