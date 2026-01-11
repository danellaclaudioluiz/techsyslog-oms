using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechsysLog.API.Controllers;
using TechsysLog.API.Models;
using TechsysLog.Application.Commands.Notifications;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Queries.Notifications;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Enums;

namespace TechsysLog.API.Tests.Controllers;

public class NotificationsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly NotificationsController _controller;
    private readonly Guid _userId;

    public NotificationsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new NotificationsController(_mediatorMock.Object);
        _userId = Guid.NewGuid();

        // Setup user context
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _userId.ToString()),
            new(ClaimTypes.Email, "user@test.com"),
            new(ClaimTypes.Role, "Customer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetAll_ShouldReturnNotifications()
    {
        // Arrange
        var notifications = new List<NotificationDto>
        {
            new() { Id = Guid.NewGuid(), Message = "Order created", Type = NotificationType.OrderCreated, Read = false },
            new() { Id = Guid.NewGuid(), Message = "Order delivered", Type = NotificationType.OrderDelivered, Read = true }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserNotificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _controller.GetAll(false, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<NotificationDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithUnreadOnlyFilter_ShouldPassFilterToQuery()
    {
        // Arrange
        var notifications = new List<NotificationDto>
        {
            new() { Id = Guid.NewGuid(), Message = "Order created", Type = NotificationType.OrderCreated, Read = false }
        };

        GetUserNotificationsQuery? capturedQuery = null;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserNotificationsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IEnumerable<NotificationDto>>, CancellationToken>((q, _) => capturedQuery = q as GetUserNotificationsQuery)
            .ReturnsAsync(notifications);

        // Act
        await _controller.GetAll(true, CancellationToken.None);

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.UnreadOnly.Should().BeTrue();
        capturedQuery.UserId.Should().Be(_userId);
    }

    [Fact]
    public async Task GetUnreadCount_ShouldReturnCount()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUnreadCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _controller.GetUnreadCount(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<UnreadCountResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Count.Should().Be(5);
    }

    [Fact]
    public async Task MarkAsRead_WithValidNotification_ShouldReturnSuccess()
    {
        // Arrange
        var notificationId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<MarkNotificationAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(MediatR.Unit.Value));

        // Act
        var result = await _controller.MarkAsRead(notificationId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsRead_WithNonExistentNotification_ShouldReturnNotFound()
    {
        // Arrange
        var notificationId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<MarkNotificationAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<MediatR.Unit>("Notification not found."));

        // Act
        var result = await _controller.MarkAsRead(notificationId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task MarkAsRead_WithDifferentUserNotification_ShouldReturnBadRequest()
    {
        // Arrange
        var notificationId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<MarkNotificationAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<MediatR.Unit>("You can only mark your own notifications as read."));

        // Act
        var result = await _controller.MarkAsRead(notificationId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldReturnSuccess()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<MarkAllNotificationsAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(MediatR.Unit.Value));

        // Act
        var result = await _controller.MarkAllAsRead(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldPassCorrectUserId()
    {
        // Arrange
        MarkAllNotificationsAsReadCommand? capturedCommand = null;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<MarkAllNotificationsAsReadCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<MediatR.Unit>>, CancellationToken>((c, _) => capturedCommand = c as MarkAllNotificationsAsReadCommand)
            .ReturnsAsync(Result.Success(MediatR.Unit.Value));

        // Act
        await _controller.MarkAllAsRead(CancellationToken.None);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.UserId.Should().Be(_userId);
    }
}