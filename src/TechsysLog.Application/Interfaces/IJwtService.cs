using TechsysLog.Domain.Entities;

namespace TechsysLog.Application.Interfaces;

/// <summary>
/// Service interface for JWT token generation and validation.
/// Implemented in Infrastructure layer.
/// </summary>
public interface IJwtService
{
    string GenerateToken(User user);
    int GetExpirationInSeconds();
}