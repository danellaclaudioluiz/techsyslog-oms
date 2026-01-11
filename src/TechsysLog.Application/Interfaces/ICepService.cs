using TechsysLog.Domain.Common;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Interfaces;

/// <summary>
/// Service interface for CEP lookup.
/// </summary>
public interface ICepService
{
    Task<Result<CepAddressInfo>> GetAddressByCepAsync(Cep cep, CancellationToken cancellationToken = default);
}

/// <summary>
/// Address information returned by CEP lookup (without number).
/// </summary>
public sealed record CepAddressInfo(
    string Street,
    string Neighborhood,
    string City,
    string State);