using System.Text.RegularExpressions;
using TechsysLog.Domain.Common;

namespace TechsysLog.Domain.ValueObjects;

/// <summary>
/// Represents a valid email address.
/// Encapsulates email validation rules as per RFC 5322 simplified pattern.
/// </summary>
public sealed partial class Email : ValueObject
{
    private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

    private Email(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public string Value { get; }

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>("Email is required.");

        var trimmed = email.Trim();

        if (trimmed.Length > 256)
            return Result.Failure<Email>("Email must not exceed 256 characters.");

        if (!EmailRegex().IsMatch(trimmed))
            return Result.Failure<Email>("Email format is invalid.");

        return new Email(trimmed);
    }

    [GeneratedRegex(EmailPattern)]
    private static partial Regex EmailRegex();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}