using FluentAssertions;
using NSubstitute;
using TechsysLog.Application.Queries.Notifications;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Tests.Queries.Notifications;

public class GetUnreadCountQueryHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly GetUnreadCountQueryHandler _handler;

    public GetUnreadCountQueryHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _handler = new GetUnreadCountQueryHandler(_notificationRepository);
    }

    [Fact]
    public async Task Handle_WithUnreadNotifications_ShouldReturnCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUnreadCountQuery { UserId = userId };

        _notificationRepository.GetUnreadCountByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(5);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WithNoUnreadNotifications_ShouldReturnZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUnreadCountQuery { UserId = userId };

        _notificationRepository.GetUnreadCountByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUnreadCountQuery { UserId = userId };

        _notificationRepository.GetUnreadCountByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _notificationRepository.Received(1).GetUnreadCountByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }
}