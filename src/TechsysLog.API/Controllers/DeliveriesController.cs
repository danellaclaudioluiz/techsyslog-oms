using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.API.Models;
using TechsysLog.Application.Commands.Deliveries;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Queries.Deliveries;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Delivery management endpoints.
/// </summary>
[Authorize]
public class DeliveriesController : BaseController
{
    private readonly IMediator _mediator;

    public DeliveriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a delivery for an order.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "OperatorOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDeliveryRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterDeliveryCommand
        {
            OrderId = request.OrderId,
            DeliveredBy = CurrentUserId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to register delivery."));

        return CreatedAtAction(
            nameof(GetByOrderId),
            new { orderId = result.Value.OrderId },
            ApiResponse<DeliveryDto>.Ok(result.Value, "Delivery registered successfully."));
    }

    /// <summary>
    /// Get delivery by order ID.
    /// </summary>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var query = new GetDeliveryByOrderIdQuery { OrderId = orderId };
        var delivery = await _mediator.Send(query, cancellationToken);

        if (delivery is null)
            return NotFoundResponse("Delivery not found for this order.");

        return Ok(ApiResponse<DeliveryDto>.Ok(delivery));
    }
}

/// <summary>
/// Register delivery request model.
/// </summary>
public class RegisterDeliveryRequest
{
    public Guid OrderId { get; set; }
}