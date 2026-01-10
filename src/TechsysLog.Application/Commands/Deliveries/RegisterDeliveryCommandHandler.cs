using AutoMapper;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Commands.Deliveries;

/// <summary>
/// Handler for RegisterDeliveryCommand.
/// Creates delivery record and updates order status to Delivered.
/// </summary>
public sealed class RegisterDeliveryCommandHandler : ICommandHandler<RegisterDeliveryCommand, DeliveryDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IMapper _mapper;

    public RegisterDeliveryCommandHandler(
        IOrderRepository orderRepository,
        IDeliveryRepository deliveryRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _deliveryRepository = deliveryRepository;
        _mapper = mapper;
    }

    public async Task<Result<DeliveryDto>> Handle(RegisterDeliveryCommand request, CancellationToken cancellationToken)
    {
        // Find order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<DeliveryDto>("Order not found.");

        // Check if delivery already exists
        if (await _deliveryRepository.OrderHasDeliveryAsync(request.OrderId, cancellationToken))
            return Result.Failure<DeliveryDto>("Order already has a delivery record.");

        // Create delivery (domain validates order status)
        var deliveryResult = Delivery.Create(order, request.DeliveredBy);
        if (deliveryResult.IsFailure)
            return Result.Failure<DeliveryDto>(deliveryResult.Error!);

        // Update order status to Delivered
        var statusResult = order.UpdateStatus(OrderStatus.Delivered);
        if (statusResult.IsFailure)
            return Result.Failure<DeliveryDto>(statusResult.Error!);

        // Persist changes
        await _deliveryRepository.AddAsync(deliveryResult.Value, cancellationToken);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        // Map to DTO and return
        var deliveryDto = _mapper.Map<DeliveryDto>(deliveryResult.Value);
        return Result.Success(deliveryDto);
    }
}