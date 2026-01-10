namespace TechsysLog.Application.DTOs;

/// <summary>
/// Data transfer object for authentication response.
/// Contains JWT token and user information.
/// </summary>
public sealed record AuthDto
{
    public string AccessToken { get; init; } = null!;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public UserDto User { get; init; } = null!;
}