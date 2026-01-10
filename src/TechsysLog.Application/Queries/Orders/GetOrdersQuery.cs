using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.Queries.Orders;

/// <summary>
/// Query to get orders with filtering and cursor-based pagination.
/// </summary>
public sealed record GetOrdersQuery : IQuery<PagedResult<OrderDto>>
{
    public Guid? UserId { get; init; }
    public OrderStatus? Status { get; init; }
    public string? Cursor { get; init; }
    public int Limit { get; init; } = 20;
}