using AutoMapper;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Queries.Notifications;

/// <summary>
/// Handler for GetUserNotificationsQuery.
/// </summary>
public sealed class GetUserNotificationsQueryHandler : IQueryHandler<GetUserNotificationsQuery, IEnumerable<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;

    public GetUserNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = request.UnreadOnly
            ? await _notificationRepository.GetUnreadByUserIdAsync(request.UserId, cancellationToken)
            : await _notificationRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }
}