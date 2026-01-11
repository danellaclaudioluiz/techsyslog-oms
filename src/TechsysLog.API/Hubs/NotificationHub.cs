using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TechsysLog.API.Hubs;

/// <summary>
/// SignalR hub for real-time notifications.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();

        if (userId != Guid.Empty)
        {
            // Add user to their personal group for targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
            _logger.LogInformation("User {UserId} connected to NotificationHub", userId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();

        if (userId != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            _logger.LogInformation("User {UserId} disconnected from NotificationHub", userId);
        }

        if (exception is not null)
        {
            _logger.LogError(exception, "User {UserId} disconnected with error", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific group (e.g., for order tracking).
    /// </summary>
    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
        _logger.LogInformation("User {UserId} joined order group {OrderId}", GetUserId(), orderId);
    }

    /// <summary>
    /// Leave a specific group.
    /// </summary>
    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
        _logger.LogInformation("User {UserId} left order group {OrderId}", GetUserId(), orderId);
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

/// <summary>
/// SignalR notification methods interface.
/// Used for strongly-typed hub clients.
/// </summary>
public interface INotificationClient
{
    Task ReceiveNotification(NotificationMessage notification);
    Task OrderStatusChanged(OrderStatusMessage message);
    Task UnreadCountUpdated(int count);
}

/// <summary>
/// Notification message model for SignalR.
/// </summary>
public class NotificationMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public object? Data { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Order status change message model for SignalR.
/// </summary>
public class OrderStatusMessage
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string OldStatus { get; set; } = null!;
    public string NewStatus { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
}