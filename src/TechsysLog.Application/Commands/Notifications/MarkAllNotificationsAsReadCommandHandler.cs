using MediatR;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Commands.Notifications;

/// <summary>
/// Handler for MarkAllNotificationsAsReadCommand.
/// </summary>
public sealed class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Result<Unit>>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkAllNotificationsAsReadCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<Unit>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        await _notificationRepository.MarkAllAsReadAsync(request.UserId, cancellationToken);

        return Result.Success(Unit.Value);
    }
}