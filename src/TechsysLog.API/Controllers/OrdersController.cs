using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.API.Models;
using TechsysLog.Application.Commands.Orders;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Queries.Orders;
using TechsysLog.Domain.Enums;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Order management endpoints.
/// </summary>
[Authorize]
public class OrdersController : BaseController
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new order.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "OperatorOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand
        {
            Description = request.Description,
            Value = request.Value,
            UserId = request.UserId ?? CurrentUserId,
            Cep = request.Cep,
            Number = request.Number,
            Complement = request.Complement
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to create order."));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            ApiResponse<OrderDto>.Ok(result.Value, "Order created successfully."));
    }

    /// <summary>
    /// Get all orders with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] OrderStatus? status,
        [FromQuery] Guid? userId,
        [FromQuery] string? cursor,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersQuery
        {
            Status = status,
            UserId = CurrentUserRole == "Customer" ? CurrentUserId : userId,
            Cursor = cursor,
            Limit = limit
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result));
    }

    /// <summary>
    /// Get order by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery { OrderId = id };
        var order = await _mediator.Send(query, cancellationToken);

        if (order is null)
            return NotFoundResponse("Order not found.");

        // Customers can only view their own orders
        if (CurrentUserRole == "Customer" && order.UserId != CurrentUserId)
            return ForbiddenResponse("You can only view your own orders.");

        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    /// <summary>
    /// Update order status.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "OperatorOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
            return BadRequest(ApiResponse.Fail("Invalid order status."));

        var command = new UpdateOrderStatusCommand
        {
            OrderId = id,
            NewStatus = status
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found") == true)
                return NotFoundResponse(result.Error);

            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to update order status."));
        }

        return Ok(ApiResponse<OrderDto>.Ok(result.Value, "Order status updated successfully."));
    }

    /// <summary>
    /// Cancel order.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand { OrderId = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found") == true)
                return NotFoundResponse(result.Error);

            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to cancel order."));
        }

        return Ok(ApiResponse.Ok("Order cancelled successfully."));
    }
}

/// <summary>
/// Create order request model.
/// </summary>
public class CreateOrderRequest
{
    public string Description { get; set; } = null!;
    public decimal Value { get; set; }
    public Guid? UserId { get; set; }
    public string Cep { get; set; } = null!;
    public string Number { get; set; } = null!;
    public string? Complement { get; set; }
}

/// <summary>
/// Update order status request model.
/// </summary>
public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = null!;
}