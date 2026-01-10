using FluentAssertions;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Domain.Tests.Entities;

public class NotificationTests
{
    private static Guid ValidUserId => Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = Notification.Create(
            userId,
            NotificationType.OrderCreated,
            "Your order has been created.");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.Type.Should().Be(NotificationType.OrderCreated);
        result.Value.Message.Should().Be("Your order has been created.");
        result.Value.Data.Should().BeNull();
        result.Value.Read.Should().BeFalse();
        result.Value.ReadAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithData_ShouldReturnSuccess()
    {
        // Arrange
        var data = "{\"orderId\":\"123\",\"value\":100.50}";

        // Act
        var result = Notification.Create(
            ValidUserId,
            NotificationType.OrderCreated,
            "Your order has been created.",
            data);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().Be(data);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        // Act
        var result = Notification.Create(
            Guid.Empty,
            NotificationType.OrderCreated,
            "Your order has been created.");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User ID is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidMessage_ShouldReturnFailure(string? message)
    {
        // Act
        var result = Notification.Create(
            ValidUserId,
            NotificationType.OrderCreated,
            message);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Message is required.");
    }

    [Fact]
    public void Create_WithMessageExceeding500Characters_ShouldReturnFailure()
    {
        // Arrange
        var message = new string('a', 501);

        // Act
        var result = Notification.Create(
            ValidUserId,
            NotificationType.OrderCreated,
            message);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Message must not exceed 500 characters.");
    }

    [Fact]
    public void Create_WithDataExceeding4000Characters_ShouldReturnFailure()
    {
        // Arrange
        var data = new string('a', 4001);

        // Act
        var result = Notification.Create(
            ValidUserId,
            NotificationType.OrderCreated,
            "Test message",
            data);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Data must not exceed 4000 characters.");
    }

    [Fact]
    public void MarkAsRead_WhenUnread_ShouldReturnSuccess()
    {
        // Arrange
        var notification = Notification.Create(
            ValidUserId,
            NotificationType.OrderCreated,
            "Test message").Value;

        // Act
        var result = notification.MarkAsRead();

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.Read.Should().BeTrue();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_ShouldReturnFailure()
    {
        // Arrange
        var notification = Notification.Create(
            ValidUserId,
            NotificationType.OrderCreated,
            "Test message").Value;
        notification.MarkAsRead();

        // Act
        var result = notification.MarkAsRead();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Notification is already read.");
    }

    [Fact]
    public void MarkAsUnread_WhenRead_ShouldReturnSuccess()
    {
        // Arrange
        var notification = Notification.Create(
            ValidUserId,
            NotificationType.OrderCreated,
            "Test message").Value;
        notification.MarkAsRead();

        // Act
        var result = notification.MarkAsUnread();

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.Read.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsUnread_WhenAlreadyUnread_ShouldReturnFailure()
    {
        // Arrange
        var notification = Notification.Create(
            ValidUserId,
            NotificationType.OrderCreated,
            "Test message").Value;

        // Act
        var result = notification.MarkAsUnread();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Notification is already unread.");
    }

    [Theory]
    [InlineData(NotificationType.OrderCreated)]
    [InlineData(NotificationType.OrderStatusChanged)]
    [InlineData(NotificationType.OrderDelivered)]
    public void Create_WithAllNotificationTypes_ShouldReturnSuccess(NotificationType type)
    {
        // Act
        var result = Notification.Create(
            ValidUserId,
            type,
            "Test message");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(type);
    }
}