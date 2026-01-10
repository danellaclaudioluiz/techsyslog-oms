namespace TechsysLog.Domain.Enums;

/// <summary>
/// Types of notifications sent to users.
/// Each type triggers a specific SignalR event.
/// </summary>
public enum NotificationType
{
    OrderCreated = 1,
    OrderStatusChanged = 2,
    OrderDelivered = 3
}