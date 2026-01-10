namespace TechsysLog.Application.Interfaces;

/// <summary>
/// Service interface for password hashing and verification.
/// Implemented in Infrastructure layer using BCrypt.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}