using FluentValidation;

namespace TechsysLog.Application.Commands.Users;

/// <summary>
/// Validator for LoginCommand.
/// Basic validation - detailed auth errors are handled in handler.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}