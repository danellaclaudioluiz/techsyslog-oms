using AutoMapper;
using MediatR;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Events;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Commands.Orders;

public sealed class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IMapper mapper,
        INotificationService notificationService,
        IMediator mediator)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _notificationService = notificationService;
        _mediator = mediator;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<OrderDto>("Order not found.");

        var oldStatus = order.Status;

        var result = order.UpdateStatus(request.NewStatus);
        if (result.IsFailure)
            return Result.Failure<OrderDto>(result.Error!);

        await _orderRepository.UpdateAsync(order, cancellationToken);

        var statusChangedEvent = new OrderStatusChangedEvent(
            order.Id,
            order.OrderNumber.Value,
            order.UserId,
            oldStatus,
            request.NewStatus);

        await _mediator.Publish(statusChangedEvent, cancellationToken);

        var orderDto = _mapper.Map<OrderDto>(order);
        return Result.Success(orderDto);
    }
}