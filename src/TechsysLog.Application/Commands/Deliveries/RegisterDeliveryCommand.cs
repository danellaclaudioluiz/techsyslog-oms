using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;

namespace TechsysLog.Application.Commands.Deliveries;

/// <summary>
/// Command to register a delivery for an order.
/// Order must be in InTransit status.
/// </summary>
public sealed record RegisterDeliveryCommand : ICommand<DeliveryDto>
{
    public Guid OrderId { get; init; }
    public Guid DeliveredBy { get; init; }
}