using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.API.Models;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Queries.Users;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.API.Controllers;

/// <summary>
/// User management endpoints.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class UsersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IUserRepository _userRepository;

    public UsersController(IMediator mediator, IUserRepository userRepository)
    {
        _mediator = mediator;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get all users.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        var dtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email.Value,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        });

        return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(dtos));
    }

    /// <summary>
    /// Get user by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery { UserId = id };
        var user = await _mediator.Send(query, cancellationToken);

        if (user is null)
            return NotFoundResponse("User not found.");

        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    /// <summary>
    /// Update user.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null)
            return NotFoundResponse("User not found.");

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var nameResult = user.UpdateName(request.Name);
            if (nameResult.IsFailure)
                return BadRequest(ApiResponse.Fail(nameResult.Error ?? "Failed to update name."));
        }

        // Update role if provided
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
                return BadRequest(ApiResponse.Fail("Invalid role."));

            var roleResult = user.ChangeRole(role);
            if (roleResult.IsFailure)
                return BadRequest(ApiResponse.Fail(roleResult.Error ?? "Failed to update role."));
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        var dto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email.Value,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return Ok(ApiResponse<UserDto>.Ok(dto, "User updated successfully."));
    }

    /// <summary>
    /// Delete (deactivate) user.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null)
            return NotFoundResponse("User not found.");

        await _userRepository.DeleteAsync(user, cancellationToken);

        return Ok(ApiResponse.Ok("User deleted successfully."));
    }
}

/// <summary>
/// Update user request model.
/// </summary>
public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Role { get; set; }
}