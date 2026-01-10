using TechsysLog.Application.Common;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Commands.Orders;

/// <summary>
/// Handler for CancelOrderCommand.
/// Validates cancellation rules and updates order status.
/// </summary>
public sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        // Find order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure("Order not found.");

        // Cancel order (domain validates rules)
        var result = order.Cancel();
        if (result.IsFailure)
            return result;

        // Persist changes
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}