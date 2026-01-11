using System.Text.Json;
using MediatR;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Events;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.EventHandlers;

/// <summary>
/// Handles OrderDeliveredEvent to create notification and send via SignalR.
/// </summary>
public sealed class OrderDeliveredEventHandler : INotificationHandler<OrderDeliveredEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationService _notificationService;

    public OrderDeliveredEventHandler(
        INotificationRepository notificationRepository,
        INotificationService notificationService)
    {
        _notificationRepository = notificationRepository;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderDeliveredEvent domainEvent, CancellationToken cancellationToken)
    {
        var message = $"Pedido {domainEvent.OrderNumber} foi entregue!";
        var data = JsonSerializer.Serialize(new
        {
            domainEvent.OrderId,
            domainEvent.OrderNumber,
            domainEvent.DeliveryId,
            domainEvent.DeliveredAt
        });

        var notificationResult = Notification.Create(
            domainEvent.UserId,
            NotificationType.OrderDelivered,
            message,
            data);

        if (notificationResult.IsSuccess)
        {
            await _notificationRepository.AddAsync(notificationResult.Value, cancellationToken);

            await _notificationService.SendToUserAsync(
                domainEvent.UserId,
                notificationResult.Value,
                cancellationToken);
        }
    }
}