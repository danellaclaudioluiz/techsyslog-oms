using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;

namespace TechsysLog.Application.Queries.Notifications;

/// <summary>
/// Query to get notifications for a user.
/// </summary>
public sealed record GetUserNotificationsQuery : IQuery<IEnumerable<NotificationDto>>
{
    public Guid UserId { get; init; }
    public bool UnreadOnly { get; init; } = false;
}