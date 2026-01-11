using AutoMapper;
using MediatR;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Events;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Commands.Orders;

/// <summary>
/// Handler for CreateOrderCommand.
/// Fetches address from CEP and creates order with generated number.
/// </summary>
public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICepService _cepService;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICepService cepService,
        IMapper mapper,
        IMediator mediator)
    {
        _orderRepository = orderRepository;
        _cepService = cepService;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Create CEP value object
        var cepResult = Cep.Create(request.Cep);
        if (cepResult.IsFailure)
            return Result.Failure<OrderDto>(cepResult.Error!);

        // Fetch address info from ViaCEP
        var addressInfoResult = await _cepService.GetAddressByCepAsync(cepResult.Value, cancellationToken);
        if (addressInfoResult.IsFailure)
            return Result.Failure<OrderDto>(addressInfoResult.Error!);

        var addressInfo = addressInfoResult.Value;

        // Create complete address with number and complement from request
        var completeAddressResult = Address.Create(
            cepResult.Value,
            addressInfo.Street,
            request.Number,
            addressInfo.Neighborhood,
            addressInfo.City,
            addressInfo.State,
            request.Complement);

        if (completeAddressResult.IsFailure)
            return Result.Failure<OrderDto>(completeAddressResult.Error!);

        // Generate order number based on daily sequence
        var today = DateTime.UtcNow.Date;
        var dailyCount = await _orderRepository.GetDailyOrderCountAsync(today, cancellationToken);
        var orderNumber = OrderNumber.Generate(dailyCount + 1);

        // Create order entity
        var orderResult = Order.Create(
            orderNumber,
            request.Description,
            request.Value,
            completeAddressResult.Value,
            request.UserId);

        if (orderResult.IsFailure)
            return Result.Failure<OrderDto>(orderResult.Error!);

        var order = orderResult.Value;

        // Persist order
        await _orderRepository.AddAsync(order, cancellationToken);

        // Publish domain event for notifications
        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.OrderNumber.Value,
            order.UserId,
            order.Value);

        await _mediator.Publish(orderCreatedEvent, cancellationToken);

        // Map to DTO and return
        var orderDto = _mapper.Map<OrderDto>(order);
        return Result.Success(orderDto);
    }
}