namespace TechsysLog.Application.DTOs;

/// <summary>
/// Data transfer object for Delivery entity.
/// </summary>
public sealed record DeliveryDto
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid DeliveredBy { get; init; }
    public DateTime DeliveredAt { get; init; }
    public DateTime CreatedAt { get; init; }
}