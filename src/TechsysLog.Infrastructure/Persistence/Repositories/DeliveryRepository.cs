using System.Linq.Expressions;
using MongoDB.Driver;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of IDeliveryRepository.
/// </summary>
public class DeliveryRepository : IDeliveryRepository
{
    private readonly IMongoCollection<Delivery> _collection;

    public DeliveryRepository(MongoDbContext context)
    {
        _collection = context.Deliveries;
    }

    public async Task<Delivery?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(d => d.Id == id && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Delivery>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(d => !d.IsDeleted)
            .SortByDescending(d => d.DeliveredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Delivery>> FindAsync(Expression<Func<Delivery, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Delivery>.Filter.And(
            Builders<Delivery>.Filter.Where(predicate),
            Builders<Delivery>.Filter.Eq(d => d.IsDeleted, false));

        return await _collection
            .Find(filter)
            .ToListAsync(cancellationToken);
    }

    public async Task<Delivery> AddAsync(Delivery entity, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Delivery entity, CancellationToken cancellationToken = default)
    {
        entity.SetUpdated();
        await _collection.ReplaceOneAsync(
            d => d.Id == entity.Id,
            entity,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Delivery entity, CancellationToken cancellationToken = default)
    {
        entity.MarkAsDeleted(null!);
        await UpdateAsync(entity, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Delivery, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Delivery>.Filter.And(
            Builders<Delivery>.Filter.Where(predicate),
            Builders<Delivery>.Filter.Eq(d => d.IsDeleted, false));

        return await _collection
            .Find(filter)
            .AnyAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<Delivery, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var filter = predicate is null
            ? Builders<Delivery>.Filter.Eq(d => d.IsDeleted, false)
            : Builders<Delivery>.Filter.And(
                Builders<Delivery>.Filter.Where(predicate),
                Builders<Delivery>.Filter.Eq(d => d.IsDeleted, false));

        return (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<Delivery?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(d => d.OrderId == orderId && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Delivery>> GetByDeliveredByAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(d => d.DeliveredBy == userId && !d.IsDeleted)
            .SortByDescending(d => d.DeliveredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> OrderHasDeliveryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(d => d.OrderId == orderId && !d.IsDeleted)
            .AnyAsync(cancellationToken);
    }
}