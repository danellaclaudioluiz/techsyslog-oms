using AutoMapper;
using MediatR;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Events;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Commands.Orders;

/// <summary>
/// Handler for UpdateOrderStatusCommand.
/// Validates status transition and updates order.
/// </summary>
public sealed class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IMapper mapper,
        IMediator mediator)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        // Find order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<OrderDto>("Order not found.");

        var oldStatus = order.Status;

        // Update status (domain validates transition)
        var result = order.UpdateStatus(request.NewStatus);
        if (result.IsFailure)
            return Result.Failure<OrderDto>(result.Error!);

        // Persist changes
        await _orderRepository.UpdateAsync(order, cancellationToken);

        // Publish domain event for notifications
        var statusChangedEvent = new OrderStatusChangedEvent(
            order.Id,
            order.OrderNumber.Value,
            order.UserId,
            oldStatus,
            request.NewStatus);

        await _mediator.Publish(statusChangedEvent, cancellationToken);

        // Map to DTO and return
        var orderDto = _mapper.Map<OrderDto>(order);
        return Result.Success(orderDto);
    }
}