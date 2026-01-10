using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;

namespace TechsysLog.Application.Queries.Deliveries;

/// <summary>
/// Query to get a delivery by order ID.
/// </summary>
public sealed record GetDeliveryByOrderIdQuery : IQuery<DeliveryDto?>
{
    public Guid OrderId { get; init; }
}