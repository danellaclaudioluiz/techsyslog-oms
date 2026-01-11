using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechsysLog.API.Controllers;
using TechsysLog.API.Models;
using TechsysLog.Application.Commands.Deliveries;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Queries.Deliveries;
using TechsysLog.Domain.Common;

namespace TechsysLog.API.Tests.Controllers;

public class DeliveriesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly DeliveriesController _controller;
    private readonly Guid _userId;

    public DeliveriesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new DeliveriesController(_mediatorMock.Object);
        _userId = Guid.NewGuid();

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
    public async Task Register_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new RegisterDeliveryRequest { OrderId = orderId };

        var deliveryDto = new DeliveryDto
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            DeliveredBy = _userId,
            DeliveredAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RegisterDeliveryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(deliveryDto));

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<ApiResponse<DeliveryDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task Register_WithInvalidOrder_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterDeliveryRequest { OrderId = Guid.NewGuid() };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RegisterDeliveryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<DeliveryDto>("Order not found."));

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Register_ShouldUseCurrentUserAsDeliveredBy()
    {
        // Arrange
        var request = new RegisterDeliveryRequest { OrderId = Guid.NewGuid() };

        RegisterDeliveryCommand? capturedCommand = null;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RegisterDeliveryCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<DeliveryDto>>, CancellationToken>((c, _) => capturedCommand = c as RegisterDeliveryCommand)
            .ReturnsAsync(Result.Success(new DeliveryDto()));

        // Act
        await _controller.Register(request, CancellationToken.None);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.DeliveredBy.Should().Be(_userId);
    }

    [Fact]
    public async Task GetByOrderId_WithExistingDelivery_ShouldReturnDelivery()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var deliveryDto = new DeliveryDto
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            DeliveredBy = _userId,
            DeliveredAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetDeliveryByOrderIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryDto);

        // Act
        var result = await _controller.GetByOrderId(orderId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<DeliveryDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetByOrderId_WithNonExistentDelivery_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetDeliveryByOrderIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DeliveryDto?)null);

        // Act
        var result = await _controller.GetByOrderId(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}