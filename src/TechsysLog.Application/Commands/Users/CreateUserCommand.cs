using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.Commands.Users;

/// <summary>
/// Command to create a new user.
/// </summary>
public sealed record CreateUserCommand : ICommand<UserDto>
{
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public UserRole Role { get; init; } = UserRole.Customer;
}