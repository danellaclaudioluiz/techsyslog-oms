using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.API.Models;
using TechsysLog.Domain.Common;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Base controller with common functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current authenticated user's ID.
    /// </summary>
    protected Guid CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>
    /// Gets the current authenticated user's email.
    /// </summary>
    protected string? CurrentUserEmail => User.FindFirst(ClaimTypes.Email)?.Value;

    /// <summary>
    /// Gets the current authenticated user's role.
    /// </summary>
    protected string? CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value;

    /// <summary>
    /// Returns an appropriate response based on the Result pattern.
    /// </summary>
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Ok(result.Value));

        return BadRequest(ApiResponse<T>.Fail(result.Error));
    }

    /// <summary>
    /// Returns an appropriate response based on the Result pattern.
    /// </summary>
    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse.Ok());

        return BadRequest(ApiResponse.Fail(result.Error));
    }

    /// <summary>
    /// Returns a created response with location header.
    /// </summary>
    protected IActionResult Created<T>(T data, string actionName, object routeValues)
    {
        return CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(data));
    }

    /// <summary>
    /// Returns a not found response.
    /// </summary>
    protected IActionResult NotFoundResponse(string message = "Resource not found.")
    {
        return NotFound(ApiResponse.Fail(message));
    }

    /// <summary>
    /// Returns an unauthorized response.
    /// </summary>
    protected IActionResult UnauthorizedResponse(string message = "Unauthorized access.")
    {
        return Unauthorized(ApiResponse.Fail(message));
    }

    /// <summary>
    /// Returns a forbidden response.
    /// </summary>
    protected IActionResult ForbiddenResponse(string message = "Access denied.")
    {
        return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail(message));
    }
}