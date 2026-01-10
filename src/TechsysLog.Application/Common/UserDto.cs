using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.DTOs;

/// <summary>
/// Data transfer object for User entity.
/// Used for API responses - never exposes password hash.
/// </summary>
public sealed record UserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public UserRole Role { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}