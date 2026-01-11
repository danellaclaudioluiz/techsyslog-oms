using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.Interfaces;

/// <summary>
/// Service interface for real-time notifications via SignalR.
/// Implemented in Infrastructure layer.
/// </summary>
public interface INotificationService
{
    Task SendToUserAsync(Guid userId, NotificationType type, string message, object? data = null, CancellationToken cancellationToken = default);
    Task SendToUserAsync(Guid userId, Notification notification, CancellationToken cancellationToken = default);
    Task SendToAllAsync(NotificationType type, string message, object? data = null, CancellationToken cancellationToken = default);
    Task SendUnreadCountAsync(Guid userId, int count, CancellationToken cancellationToken = default);
}