using FluentAssertions;
using Moq;
using TechsysLog.Application.Commands.Notifications;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Tests.Commands.Notifications;

public class MarkAllNotificationsAsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly MarkAllNotificationsAsReadCommandHandler _handler;

    public MarkAllNotificationsAsReadCommandHandlerTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _handler = new MarkAllNotificationsAsReadCommandHandler(_notificationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldMarkAllAsRead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new MarkAllNotificationsAsReadCommand { UserId = userId };

        _notificationRepositoryMock
            .Setup(r => r.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationRepositoryMock.Verify(
            r => r.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new MarkAllNotificationsAsReadCommand { UserId = userId };

        Guid capturedUserId = Guid.Empty;
        _notificationRepositoryMock
            .Setup(r => r.MarkAllAsReadAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, CancellationToken>((id, _) => capturedUserId = id)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_WithNoUnreadNotifications_ShouldStillReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new MarkAllNotificationsAsReadCommand { UserId = userId };

        _notificationRepositoryMock
            .Setup(r => r.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}