using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces;

/// <summary>
/// Repository interface for Delivery aggregate.
/// Extends generic repository with delivery-specific operations.
/// </summary>
public interface IDeliveryRepository : IRepository<Delivery>
{
    Task<Delivery?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Delivery>> GetByDeliveredByAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> OrderHasDeliveryAsync(Guid orderId, CancellationToken cancellationToken = default);
}