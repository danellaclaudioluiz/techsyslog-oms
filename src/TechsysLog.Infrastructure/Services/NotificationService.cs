using Microsoft.AspNetCore.SignalR;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Infrastructure.Hubs;

namespace TechsysLog.Infrastructure.Services;

/// <summary>
/// SignalR implementation of INotificationService.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(Guid userId, NotificationType type, string message, object? data = null, CancellationToken cancellationToken = default)
    {
        var notification = new
        {
            Type = type.ToString(),
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("ReceiveNotification", notification, cancellationToken);
    }

    public async Task SendToUserAsync(Guid userId, Notification notification, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            Id = notification.Id,
            Type = notification.Type.ToString(),
            Message = notification.Message,
            Data = notification.Data,
            Read = notification.Read,
            CreatedAt = notification.CreatedAt
        };

        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("ReceiveNotification", payload, cancellationToken);
    }

    public async Task SendToAllAsync(NotificationType type, string message, object? data = null, CancellationToken cancellationToken = default)
    {
        var notification = new
        {
            Type = type.ToString(),
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.All
            .SendAsync("ReceiveNotification", notification, cancellationToken);
    }

    public async Task SendUnreadCountAsync(Guid userId, int count, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("UnreadCount", count, cancellationToken);
    }
}