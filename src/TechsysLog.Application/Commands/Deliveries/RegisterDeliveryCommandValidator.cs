using FluentValidation;

namespace TechsysLog.Application.Commands.Deliveries;

/// <summary>
/// Validator for RegisterDeliveryCommand.
/// </summary>
public sealed class RegisterDeliveryCommandValidator : AbstractValidator<RegisterDeliveryCommand>
{
    public RegisterDeliveryCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.DeliveredBy)
            .NotEmpty().WithMessage("DeliveredBy is required.");
    }
}