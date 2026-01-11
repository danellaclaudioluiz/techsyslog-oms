using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.API.Models;
using TechsysLog.Application.Commands.Notifications;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Queries.Notifications;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Notification management endpoints.
/// </summary>
[Authorize]
public class NotificationsController : BaseController
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user's notifications.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool unreadOnly = false, CancellationToken cancellationToken = default)
    {
        var query = new GetUserNotificationsQuery
        {
            UserId = CurrentUserId,
            UnreadOnly = unreadOnly
        };

        var notifications = await _mediator.Send(query, cancellationToken);

        return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(notifications));
    }

    /// <summary>
    /// Get unread notifications count.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<UnreadCountResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var query = new GetUnreadCountQuery { UserId = CurrentUserId };
        var count = await _mediator.Send(query, cancellationToken);

        return Ok(ApiResponse<UnreadCountResponse>.Ok(new UnreadCountResponse { Count = count }));
    }

    /// <summary>
    /// Mark notification as read.
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = id,
            UserId = CurrentUserId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found") == true)
                return NotFoundResponse(result.Error);

            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to mark notification as read."));
        }

        return Ok(ApiResponse.Ok("Notification marked as read."));
    }

    /// <summary>
    /// Mark all notifications as read.
    /// </summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var command = new MarkAllNotificationsAsReadCommand { UserId = CurrentUserId };
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to mark notifications as read."));

        return Ok(ApiResponse.Ok("All notifications marked as read."));
    }
}

/// <summary>
/// Unread count response model.
/// </summary>
public class UnreadCountResponse
{
    public int Count { get; set; }
}