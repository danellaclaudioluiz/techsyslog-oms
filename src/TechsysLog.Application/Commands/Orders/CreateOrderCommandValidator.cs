using FluentValidation;

namespace TechsysLog.Application.Commands.Orders;

/// <summary>
/// Validator for CreateOrderCommand.
/// </summary>
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Value must be greater than zero.");

        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("CEP is required.")
            .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP format is invalid.");

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Number is required.")
            .MaximumLength(20).WithMessage("Number must not exceed 20 characters.");

        RuleFor(x => x.Complement)
            .MaximumLength(100).WithMessage("Complement must not exceed 100 characters.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}