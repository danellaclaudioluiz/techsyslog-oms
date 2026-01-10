using FluentAssertions;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Infrastructure.Persistence.Repositories;
using TechsysLog.Integration.Tests.Fixtures;

namespace TechsysLog.Integration.Tests.Repositories;

public class NotificationRepositoryIntegrationTests : IClassFixture<MongoDbFixture>
{
    private readonly NotificationRepository _repository;

    public NotificationRepositoryIntegrationTests(MongoDbFixture fixture)
    {
        _repository = new NotificationRepository(fixture.Context);
    }

    private static Notification CreateTestNotification(Guid? userId = null, bool read = false)
    {
        var notification = Notification.Create(
            userId ?? Guid.NewGuid(),
            NotificationType.OrderCreated,
            "Test notification message",
            "{\"orderId\":\"123\"}").Value;

        if (read)
            notification.MarkAsRead();

        return notification;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistNotification()
    {
        // Arrange
        var notification = CreateTestNotification();

        // Act
        var result = await _repository.AddAsync(notification);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(notification.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingNotification_ShouldReturnNotification()
    {
        // Arrange
        var notification = CreateTestNotification();
        await _repository.AddAsync(notification);

        // Act
        var result = await _repository.GetByIdAsync(notification.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(notification.Id);
        result.Message.Should().Be(notification.Message);
        result.Type.Should().Be(NotificationType.OrderCreated);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingNotification_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification1 = CreateTestNotification(userId);
        var notification2 = CreateTestNotification(userId);
        var notification3 = CreateTestNotification(Guid.NewGuid());
        await _repository.AddAsync(notification1);
        await _repository.AddAsync(notification2);
        await _repository.AddAsync(notification3);

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(n => n.Id == notification1.Id);
        result.Should().Contain(n => n.Id == notification2.Id);
        result.Should().NotContain(n => n.Id == notification3.Id);
    }

    [Fact]
    public async Task GetUnreadByUserIdAsync_ShouldReturnOnlyUnreadNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification1 = CreateTestNotification(userId, read: false);
        var notification2 = CreateTestNotification(userId, read: true);
        var notification3 = CreateTestNotification(userId, read: false);
        await _repository.AddAsync(notification1);
        await _repository.AddAsync(notification2);
        await _repository.AddAsync(notification3);

        // Act
        var result = await _repository.GetUnreadByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(n => n.Id == notification1.Id);
        result.Should().Contain(n => n.Id == notification3.Id);
        result.Should().NotContain(n => n.Id == notification2.Id);
    }

    [Fact]
    public async Task GetUnreadCountByUserIdAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification1 = CreateTestNotification(userId, read: false);
        var notification2 = CreateTestNotification(userId, read: true);
        var notification3 = CreateTestNotification(userId, read: false);
        await _repository.AddAsync(notification1);
        await _repository.AddAsync(notification2);
        await _repository.AddAsync(notification3);

        // Act
        var result = await _repository.GetUnreadCountByUserIdAsync(userId);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var notification = CreateTestNotification();
        await _repository.AddAsync(notification);

        notification.MarkAsRead();

        // Act
        await _repository.UpdateAsync(notification);
        var result = await _repository.GetByIdAsync(notification.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Read.Should().BeTrue();
        result.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAllAsReadAsync_ShouldMarkAllUserNotificationsAsRead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification1 = CreateTestNotification(userId, read: false);
        var notification2 = CreateTestNotification(userId, read: false);
        var notification3 = CreateTestNotification(Guid.NewGuid(), read: false);
        await _repository.AddAsync(notification1);
        await _repository.AddAsync(notification2);
        await _repository.AddAsync(notification3);

        // Act
        await _repository.MarkAllAsReadAsync(userId);

        // Assert
        var userNotifications = await _repository.GetByUserIdAsync(userId);
        userNotifications.Should().OnlyContain(n => n.Read);

        var otherNotification = await _repository.GetByIdAsync(notification3.Id);
        otherNotification!.Read.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNotificationsSortedByCreatedAtDescending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification1 = CreateTestNotification(userId);
        await _repository.AddAsync(notification1);
        await Task.Delay(10);
        var notification2 = CreateTestNotification(userId);
        await _repository.AddAsync(notification2);

        // Act
        var result = (await _repository.GetByUserIdAsync(userId)).ToList();

        // Assert
        var index1 = result.FindIndex(n => n.Id == notification1.Id);
        var index2 = result.FindIndex(n => n.Id == notification2.Id);
        index2.Should().BeLessThan(index1); // notification2 should come first (newer)
    }

    [Fact]
    public async Task GetUnreadCountByUserIdAsync_WithNoNotifications_ShouldReturnZero()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetUnreadCountByUserIdAsync(userId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetUnreadCountByUserIdAsync_ShouldNotCountDeletedNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification1 = CreateTestNotification(userId, read: false);
        var notification2 = CreateTestNotification(userId, read: false);
        await _repository.AddAsync(notification1);
        await _repository.AddAsync(notification2);

        notification1.MarkAsDeleted(null!);
        await _repository.UpdateAsync(notification1);

        // Act
        var result = await _repository.GetUnreadCountByUserIdAsync(userId);

        // Assert
        result.Should().Be(1);
    }
}