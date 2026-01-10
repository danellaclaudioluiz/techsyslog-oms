using System.Linq.Expressions;
using MongoDB.Driver;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of IUserRepository.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;

    public UserRepository(MongoDbContext context)
    {
        _collection = context.Users;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(u => u.Id == id && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(u => !u.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Where(predicate),
            Builders<User>.Filter.Eq(u => u.IsDeleted, false));

        return await _collection
            .Find(filter)
            .ToListAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.SetUpdated();
        await _collection.ReplaceOneAsync(
            u => u.Id == entity.Id,
            entity,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.MarkAsDeleted(null!);
        await UpdateAsync(entity, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Where(predicate),
            Builders<User>.Filter.Eq(u => u.IsDeleted, false));

        return await _collection
            .Find(filter)
            .AnyAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<User, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var filter = predicate is null
            ? Builders<User>.Filter.Eq(u => u.IsDeleted, false)
            : Builders<User>.Filter.And(
                Builders<User>.Filter.Where(predicate),
                Builders<User>.Filter.Eq(u => u.IsDeleted, false));

        return (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(u => u.Email == email && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(u => u.Email == email && !u.IsDeleted)
            .AnyAsync(cancellationToken);
    }
}