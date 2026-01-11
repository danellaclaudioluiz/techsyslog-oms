using System.Text.Json;
using MediatR;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Events;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.EventHandlers;

/// <summary>
/// Handles OrderStatusChangedEvent to create notification and send via SignalR.
/// </summary>
public sealed class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationService _notificationService;

    public OrderStatusChangedEventHandler(
        INotificationRepository notificationRepository,
        INotificationService notificationService)
    {
        _notificationRepository = notificationRepository;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        var message = $"Pedido {domainEvent.OrderNumber} atualizado: {domainEvent.OldStatus} → {domainEvent.NewStatus}";
        var data = JsonSerializer.Serialize(new
        {
            domainEvent.OrderId,
            domainEvent.OrderNumber,
            OldStatus = domainEvent.OldStatus.ToString(),
            NewStatus = domainEvent.NewStatus.ToString()
        });

        var notificationResult = Notification.Create(
            domainEvent.UserId,
            NotificationType.OrderStatusChanged,
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