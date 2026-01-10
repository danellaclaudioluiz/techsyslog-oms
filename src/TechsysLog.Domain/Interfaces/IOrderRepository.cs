using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Interfaces;

/// <summary>
/// Repository interface for Order aggregate.
/// Extends generic repository with order-specific operations.
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(OrderNumber orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<int> GetDailyOrderCountAsync(DateTime date, CancellationToken cancellationToken = default);
}