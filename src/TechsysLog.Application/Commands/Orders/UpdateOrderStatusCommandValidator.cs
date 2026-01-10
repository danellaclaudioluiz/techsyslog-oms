using FluentValidation;

namespace TechsysLog.Application.Commands.Orders;

/// <summary>
/// Validator for UpdateOrderStatusCommand.
/// </summary>
public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Status is invalid.");
    }
}