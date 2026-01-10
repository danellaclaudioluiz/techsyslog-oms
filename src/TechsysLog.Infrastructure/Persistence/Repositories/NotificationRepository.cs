using MongoDB.Driver;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of INotificationRepository.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _collection;

    public NotificationRepository(MongoDbContext context)
    {
        _collection = context.Notifications;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(n => n.Id == id && !n.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(n => n.UserId == userId && !n.IsDeleted)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(n => n.UserId == userId && !n.Read && !n.IsDeleted)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return (int)await _collection
            .CountDocumentsAsync(n => n.UserId == userId && !n.Read && !n.IsDeleted, cancellationToken: cancellationToken);
    }

    public async Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(notification, cancellationToken: cancellationToken);
        return notification;
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(
            n => n.Id == notification.Id,
            notification,
            cancellationToken: cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var update = Builders<Notification>.Update
            .Set(n => n.Read, true)
            .Set(n => n.ReadAt, DateTime.UtcNow);

        await _collection.UpdateManyAsync(
            n => n.UserId == userId && !n.Read && !n.IsDeleted,
            update,
            cancellationToken: cancellationToken);
    }
}