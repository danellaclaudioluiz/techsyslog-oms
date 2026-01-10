using TechsysLog.Domain.Common;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Domain.Entities;

/// <summary>
/// Represents a persistent notification for audit and history.
/// SignalR handles real-time delivery; this entity handles persistence.
/// </summary>
public sealed class Notification : BaseEntity
{
    private Notification() { } // EF/MongoDB constructor

    private Notification(
        Guid userId,
        NotificationType type,
        string message,
        string? data)
    {
        UserId = userId;
        Type = type;
        Message = message;
        Data = data;
        Read = false;
    }

    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Message { get; private set; } = null!;
    public string? Data { get; private set; }
    public bool Read { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public static Result<Notification> Create(
        Guid userId,
        NotificationType type,
        string? message,
        string? data = null)
    {
        if (userId == Guid.Empty)
            return Result.Failure<Notification>("User ID is required.");

        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure<Notification>("Message is required.");

        if (message.Length > 500)
            return Result.Failure<Notification>("Message must not exceed 500 characters.");

        if (data?.Length > 4000)
            return Result.Failure<Notification>("Data must not exceed 4000 characters.");

        var notification = new Notification(userId, type, message.Trim(), data);
        return Result.Success(notification);
    }

    public Result MarkAsRead()
    {
        if (Read)
            return Result.Failure("Notification is already read.");

        Read = true;
        ReadAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkAsUnread()
    {
        if (!Read)
            return Result.Failure("Notification is already unread.");

        Read = false;
        ReadAt = null;
        return Result.Success();
    }
}