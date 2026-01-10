using TechsysLog.Application.Common;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Queries.Notifications;

/// <summary>
/// Handler for GetUnreadCountQuery.
/// </summary>
public sealed class GetUnreadCountQueryHandler : IQueryHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _notificationRepository;

    public GetUnreadCountQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        return await _notificationRepository.GetUnreadCountByUserIdAsync(request.UserId, cancellationToken);
    }
}