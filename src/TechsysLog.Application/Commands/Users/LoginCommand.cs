using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;

namespace TechsysLog.Application.Commands.Users;

/// <summary>
/// Command to authenticate a user.
/// </summary>
public sealed record LoginCommand : ICommand<AuthDto>
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}