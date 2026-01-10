using TechsysLog.Domain.Common;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Entities;

/// <summary>
/// Represents a system user with authentication and authorization data.
/// </summary>
public sealed class User : AggregateRoot
{
    private User() { } // EF/MongoDB constructor

    private User(string name, Email email, Password password, UserRole role)
    {
        Name = name;
        Email = email;
        Password = password;
        Role = role;
    }

    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Password Password { get; private set; } = null!;
    public UserRole Role { get; private set; }

    public static Result<User> Create(
        string? name,
        Email email,
        Password password,
        UserRole role = UserRole.Customer)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>("Name is required.");

        if (name.Length > 150)
            return Result.Failure<User>("Name must not exceed 150 characters.");

        if (email is null)
            return Result.Failure<User>("Email is required.");

        if (password is null)
            return Result.Failure<User>("Password is required.");

        var user = new User(name.Trim(), email, password, role);
        return Result.Success(user);
    }

    public Result UpdateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Name is required.");

        if (name.Length > 150)
            return Result.Failure("Name must not exceed 150 characters.");

        Name = name.Trim();
        SetUpdated();
        return Result.Success();
    }

    public Result UpdateEmail(Email email)
    {
        if (email is null)
            return Result.Failure("Email is required.");

        Email = email;
        SetUpdated();
        return Result.Success();
    }

    public Result UpdatePassword(Password password)
    {
        if (password is null)
            return Result.Failure("Password is required.");

        Password = password;
        SetUpdated();
        return Result.Success();
    }

    public Result ChangeRole(UserRole newRole)
    {
        if (Role == newRole)
            return Result.Failure("User already has this role.");

        Role = newRole;
        SetUpdated();
        return Result.Success();
    }

    public bool HasPermission(UserRole requiredRole)
    {
        return Role >= requiredRole;
    }
}