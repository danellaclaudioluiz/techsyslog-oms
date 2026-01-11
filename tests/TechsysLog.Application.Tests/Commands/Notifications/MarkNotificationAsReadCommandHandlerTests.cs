using FluentAssertions;
using Moq;
using TechsysLog.Application.Commands.Notifications;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Tests.Commands.Notifications;

public class MarkNotificationAsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly MarkNotificationAsReadCommandHandler _handler;

    public MarkNotificationAsReadCommandHandlerTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _handler = new MarkNotificationAsReadCommandHandler(_notificationRepositoryMock.Object);
    }

    private static Notification CreateNotification(Guid userId, string message = "Test notification")
    {
        var result = Notification.Create(
            userId,
            NotificationType.OrderCreated,
            message,
            null);

        return result.Value;
    }

    [Fact]
    public async Task Handle_WithValidNotification_ShouldMarkAsRead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = CreateNotification(userId);

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notification.Id,
            UserId = userId
        };

        _notificationRepositoryMock
            .Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.Read.Should().BeTrue();
        _notificationRepositoryMock.Verify(
            r => r.UpdateAsync(notification, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentNotification_ShouldReturnFailure()
    {
        // Arrange
        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        _notificationRepositoryMock
            .Setup(r => r.GetByIdAsync(command.NotificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WithDifferentUserId_ShouldReturnFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var notification = CreateNotification(ownerId);

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notification.Id,
            UserId = differentUserId
        };

        _notificationRepositoryMock
            .Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("your own");
    }

    [Fact]
    public async Task Handle_WithAlreadyReadNotification_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = CreateNotification(userId);
        notification.MarkAsRead();

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notification.Id,
            UserId = userId
        };

        _notificationRepositoryMock
            .Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}