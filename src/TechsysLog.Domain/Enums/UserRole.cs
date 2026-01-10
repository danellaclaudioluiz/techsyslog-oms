namespace TechsysLog.Domain.Enums;

/// <summary>
/// Defines user access levels following the principle of least privilege.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Can only view own orders and receive notifications.
    /// </summary>
    Customer = 1,

    /// <summary>
    /// Can manage orders, register deliveries, and view reports.
    /// </summary>
    Operator = 2,

    /// <summary>
    /// Full access to all system features including user management.
    /// </summary>
    Admin = 3
}