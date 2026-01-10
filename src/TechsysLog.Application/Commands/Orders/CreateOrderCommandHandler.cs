using AutoMapper;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Entities;
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

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICepService cepService,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _cepService = cepService;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Create CEP value object
        var cepResult = Cep.Create(request.Cep);
        if (cepResult.IsFailure)
            return Result.Failure<OrderDto>(cepResult.Error!);

        // Fetch address from ViaCEP
        var addressResult = await _cepService.GetAddressByCepAsync(cepResult.Value, cancellationToken);
        if (addressResult.IsFailure)
            return Result.Failure<OrderDto>(addressResult.Error!);

        // Create complete address with number and complement
        var completeAddressResult = Address.Create(
            cepResult.Value,
            addressResult.Value.Street,
            request.Number,
            addressResult.Value.Neighborhood,
            addressResult.Value.City,
            addressResult.Value.State,
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

        // Persist order
        await _orderRepository.AddAsync(orderResult.Value, cancellationToken);

        // Map to DTO and return
        var orderDto = _mapper.Map<OrderDto>(orderResult.Value);
        return Result.Success(orderDto);
    }
}