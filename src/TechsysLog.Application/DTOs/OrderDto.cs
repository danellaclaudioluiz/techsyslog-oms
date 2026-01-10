using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.DTOs;

/// <summary>
/// Data transfer object for Order entity.
/// Includes nested address information.
/// </summary>
public sealed record OrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = null!;
    public string Description { get; init; } = null!;
    public decimal Value { get; init; }
    public OrderStatus Status { get; init; }
    public AddressDto DeliveryAddress { get; init; } = null!;
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}