using TechsysLog.Domain.Common;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Interfaces;

/// <summary>
/// Service interface for CEP lookup.
/// Implemented in Infrastructure layer using ViaCEP API.
/// </summary>
public interface ICepService
{
    Task<Result<Address>> GetAddressByCepAsync(Cep cep, CancellationToken cancellationToken = default);
}