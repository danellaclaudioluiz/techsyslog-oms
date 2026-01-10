using TechsysLog.Domain.Entities;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Interfaces;

/// <summary>
/// Repository interface for User aggregate.
/// Extends generic repository with user-specific operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);
}