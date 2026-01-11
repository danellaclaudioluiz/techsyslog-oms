using MediatR;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Commands.Notifications;

/// <summary>
/// Handler for MarkNotificationAsReadCommand.
/// </summary>
public sealed class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result<Unit>>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkNotificationAsReadCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<Unit>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification is null)
            return Result.Failure<Unit>("Notification not found.");

        if (notification.UserId != request.UserId)
            return Result.Failure<Unit>("You can only mark your own notifications as read.");

        if (notification.Read)
            return Result.Success(Unit.Value);

        notification.MarkAsRead();

        await _notificationRepository.UpdateAsync(notification, cancellationToken);

        return Result.Success(Unit.Value);
    }
}