using TechsysLog.Application.Common;

namespace TechsysLog.Application.Commands.Orders;

/// <summary>
/// Command to cancel an order.
/// Only pending and confirmed orders can be cancelled.
/// </summary>
public sealed record CancelOrderCommand : ICommand
{
    public Guid OrderId { get; init; }
}