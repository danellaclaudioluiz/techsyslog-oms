using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.DTOs;

/// <summary>
/// Data transfer object for Notification entity.
/// </summary>
public sealed record NotificationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public NotificationType Type { get; init; }
    public string Message { get; init; } = null!;
    public string? Data { get; init; }
    public bool Read { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime CreatedAt { get; init; }
}