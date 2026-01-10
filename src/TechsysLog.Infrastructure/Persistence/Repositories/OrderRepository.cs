using System.Linq.Expressions;
using MongoDB.Driver;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of IOrderRepository.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _collection;

    public OrderRepository(MongoDbContext context)
    {
        _collection = context.Orders;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(o => o.Id == id && !o.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(o => !o.IsDeleted)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> FindAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Where(predicate),
            Builders<Order>.Filter.Eq(o => o.IsDeleted, false));

        return await _collection
            .Find(filter)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order> AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        entity.SetUpdated();
        await _collection.ReplaceOneAsync(
            o => o.Id == entity.Id,
            entity,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Order entity, CancellationToken cancellationToken = default)
    {
        entity.MarkAsDeleted(null!);
        await UpdateAsync(entity, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Where(predicate),
            Builders<Order>.Filter.Eq(o => o.IsDeleted, false));

        return await _collection
            .Find(filter)
            .AnyAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<Order, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var filter = predicate is null
            ? Builders<Order>.Filter.Eq(o => o.IsDeleted, false)
            : Builders<Order>.Filter.And(
                Builders<Order>.Filter.Where(predicate),
                Builders<Order>.Filter.Eq(o => o.IsDeleted, false));

        return (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(OrderNumber orderNumber, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(o => o.OrderNumber == orderNumber && !o.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(o => o.UserId == userId && !o.IsDeleted)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(o => o.Status == status && !o.IsDeleted)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetDailyOrderCountAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Gte(o => o.CreatedAt, startOfDay),
            Builders<Order>.Filter.Lt(o => o.CreatedAt, endOfDay));

        return (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }
}