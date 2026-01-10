using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;

namespace TechsysLog.Application.Queries.Orders;

/// <summary>
/// Query to get an order by ID.
/// </summary>
public sealed record GetOrderByIdQuery : IQuery<OrderDto?>
{
    public Guid OrderId { get; init; }
}