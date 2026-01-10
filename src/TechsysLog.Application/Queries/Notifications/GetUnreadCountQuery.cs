using TechsysLog.Application.Common;

namespace TechsysLog.Application.Queries.Notifications;

/// <summary>
/// Query to get unread notifications count for a user.
/// </summary>
public sealed record GetUnreadCountQuery : IQuery<int>
{
    public Guid UserId { get; init; }
}