namespace TechsysLog.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of an order.
/// Transitions: Pending -> Confirmed -> InTransit -> Delivered
///              Pending -> Cancelled (only from Pending)
/// </summary>
public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    InTransit = 3,
    Delivered = 4,
    Cancelled = 5
}