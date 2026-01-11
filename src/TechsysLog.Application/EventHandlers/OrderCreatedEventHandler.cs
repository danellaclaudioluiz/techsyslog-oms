using System.Text.Json;
using MediatR;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Events;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.EventHandlers;

/// <summary>
/// Handles OrderCreatedEvent to create notification and send via SignalR.
/// </summary>
public sealed class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationService _notificationService;

    public OrderCreatedEventHandler(
        INotificationRepository notificationRepository,
        INotificationService notificationService)
    {
        _notificationRepository = notificationRepository;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var message = $"Pedido {domainEvent.OrderNumber} criado com sucesso!";
        var data = JsonSerializer.Serialize(new { domainEvent.OrderId, domainEvent.OrderNumber });

        var notificationResult = Notification.Create(
            domainEvent.UserId,
            NotificationType.OrderCreated,
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