using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;

namespace TechsysLog.Application.Commands.Orders;

/// <summary>
/// Command to create a new order.
/// Address is fetched automatically from CEP via ViaCEP API.
/// </summary>
public sealed record CreateOrderCommand : ICommand<OrderDto>
{
    public string Description { get; init; } = null!;
    public decimal Value { get; init; }
    public string Cep { get; init; } = null!;
    public string Number { get; init; } = null!;
    public string? Complement { get; init; }
    public Guid UserId { get; init; }
}