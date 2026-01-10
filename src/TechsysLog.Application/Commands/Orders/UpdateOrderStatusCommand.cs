using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.Commands.Orders;

/// <summary>
/// Command to update an order's status.
/// </summary>
public sealed record UpdateOrderStatusCommand : ICommand<OrderDto>
{
    public Guid OrderId { get; init; }
    public OrderStatus NewStatus { get; init; }
}