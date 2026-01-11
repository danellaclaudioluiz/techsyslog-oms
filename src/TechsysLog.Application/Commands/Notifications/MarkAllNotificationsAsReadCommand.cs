using MediatR;
using TechsysLog.Domain.Common;

namespace TechsysLog.Application.Commands.Notifications;

/// <summary>
/// Command to mark all user notifications as read.
/// </summary>
public sealed record MarkAllNotificationsAsReadCommand : IRequest<Result<Unit>>
{
    public Guid UserId { get; init; }
}