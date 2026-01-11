using MediatR;
using TechsysLog.Domain.Common;

namespace TechsysLog.Application.Commands.Notifications;

/// <summary>
/// Command to mark a notification as read.
/// </summary>
public sealed record MarkNotificationAsReadCommand : IRequest<Result<Unit>>
{
    public Guid NotificationId { get; init; }
    public Guid UserId { get; init; }
}