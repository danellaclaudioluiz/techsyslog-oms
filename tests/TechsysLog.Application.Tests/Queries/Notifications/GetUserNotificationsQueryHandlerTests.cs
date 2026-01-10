using AutoMapper;
using FluentAssertions;
using NSubstitute;
using TechsysLog.Application.Mappings;
using TechsysLog.Application.Queries.Notifications;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Tests.Queries.Notifications;

public class GetUserNotificationsQueryHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;
    private readonly GetUserNotificationsQueryHandler _handler;

    public GetUserNotificationsQueryHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new GetUserNotificationsQueryHandler(_notificationRepository, _mapper);
    }

    private static Notification CreateTestNotification(Guid userId, bool read = false)
    {
        var notification = Notification.Create(
            userId,
            NotificationType.OrderCreated,
            "Your order has been created.",
            "{\"orderId\":\"123\"}").Value;

        if (read)
            notification.MarkAsRead();

        return notification;
    }

    [Fact]
    public async Task Handle_WithUnreadOnlyFalse_ShouldReturnAllNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            CreateTestNotification(userId, read: false),
            CreateTestNotification(userId, read: true),
            CreateTestNotification(userId, read: false)
        };

        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = false
        };

        _notificationRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(notifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        await _notificationRepository.Received(1).GetByUserIdAsync(userId, Arg.Any<CancellationToken>());
        await _notificationRepository.DidNotReceive().GetUnreadByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnreadOnlyTrue_ShouldReturnOnlyUnreadNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var unreadNotifications = new List<Notification>
        {
            CreateTestNotification(userId, read: false),
            CreateTestNotification(userId, read: false)
        };

        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = true
        };

        _notificationRepository.GetUnreadByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(unreadNotifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(n => !n.Read).Should().BeTrue();

        await _notificationRepository.Received(1).GetUnreadByUserIdAsync(userId, Arg.Any<CancellationToken>());
        await _notificationRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoNotifications_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = false
        };

        _notificationRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Notification>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapNotificationCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = CreateTestNotification(userId);
        var notifications = new List<Notification> { notification };

        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = false
        };

        _notificationRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(notifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.Id.Should().Be(notification.Id);
        dto.UserId.Should().Be(userId);
        dto.Type.Should().Be(NotificationType.OrderCreated);
        dto.Message.Should().Be("Your order has been created.");
        dto.Data.Should().Be("{\"orderId\":\"123\"}");
        dto.Read.Should().BeFalse();
        dto.ReadAt.Should().BeNull();
    }
}