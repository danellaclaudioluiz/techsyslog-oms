using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.API.Models;
using TechsysLog.Application.Commands.Users;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Authentication and authorization endpoints.
/// </summary>
public class AuthController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthController(
        IMediator mediator,
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordHasher passwordHasher)
    {
        _mediator = mediator;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            role = UserRole.Customer;

        var command = new CreateUserCommand
        {
            Name = request.Name,
            Email = request.Email,
            Password = request.Password,
            Role = role
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to register user."));

        return CreatedAtAction(
            nameof(UsersController.GetById),
            "Users",
            new { id = result.Value.Id },
            ApiResponse<UserDto>.Ok(result.Value, "User registered successfully."));
    }

    /// <summary>
    /// Authenticate user and return JWT token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return UnauthorizedResponse("Invalid credentials.");

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (user is null)
            return UnauthorizedResponse("Invalid credentials.");

        if (user.IsDeleted)
            return UnauthorizedResponse("User account is deactivated.");

        if (!_passwordHasher.Verify(request.Password, user.Password.Hash))
            return UnauthorizedResponse("Invalid credentials.");

        var token = _jwtService.GenerateToken(user);

        var response = new LoginResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email.Value,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful."));
    }

    /// <summary>
    /// Get current authenticated user info.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(CurrentUserId, cancellationToken);

        if (user is null)
            return NotFoundResponse("User not found.");

        var dto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email.Value,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return Ok(ApiResponse<UserDto>.Ok(dto));
    }
}

/// <summary>
/// Register request model.
/// </summary>
public class RegisterRequest
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = "Customer";
}

/// <summary>
/// Login request model.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

/// <summary>
/// Login response model.
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}