using System.Text.RegularExpressions;
using TechsysLog.Domain.Common;

namespace TechsysLog.Domain.ValueObjects;

/// <summary>
/// Represents a password with validation rules.
/// Stores only the hash, never the plain text password.
/// </summary>
public sealed partial class Password : ValueObject
{
    private const int MinLength = 8;
    private const int MaxLength = 128;

    private Password(string hash)
    {
        Hash = hash;
    }

    public string Hash { get; }

    /// <summary>
    /// Creates a new password from plain text, validating strength requirements.
    /// </summary>
    public static Result<Password> Create(string? plainText, Func<string, string> hashFunction)
    {
        var validation = ValidatePlainText(plainText);
        if (validation.IsFailure)
            return Result.Failure<Password>(validation.Error!);

        var hash = hashFunction(plainText!);
        return new Password(hash);
    }

    /// <summary>
    /// Creates a password instance from an existing hash (e.g., from database).
    /// </summary>
    public static Password FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be empty.", nameof(hash));

        return new Password(hash);
    }

    private static Result ValidatePlainText(string? plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return Result.Failure("Password is required.");

        if (plainText.Length < MinLength)
            return Result.Failure($"Password must be at least {MinLength} characters.");

        if (plainText.Length > MaxLength)
            return Result.Failure($"Password must not exceed {MaxLength} characters.");

        if (!HasUpperCase().IsMatch(plainText))
            return Result.Failure("Password must contain at least one uppercase letter.");

        if (!HasLowerCase().IsMatch(plainText))
            return Result.Failure("Password must contain at least one lowercase letter.");

        if (!HasDigit().IsMatch(plainText))
            return Result.Failure("Password must contain at least one digit.");

        if (!HasSpecialChar().IsMatch(plainText))
            return Result.Failure("Password must contain at least one special character.");

        return Result.Success();
    }

    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex HasUpperCase();

    [GeneratedRegex(@"[a-z]")]
    private static partial Regex HasLowerCase();

    [GeneratedRegex(@"\d")]
    private static partial Regex HasDigit();

    [GeneratedRegex(@"[!@#$%^&*(),.?""':{}|<>_\-\[\]\\\/`~;=+]")]
    private static partial Regex HasSpecialChar();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Hash;
    }
}