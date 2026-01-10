using System.Text.RegularExpressions;
using TechsysLog.Domain.Common;

namespace TechsysLog.Domain.ValueObjects;

/// <summary>
/// Represents a Brazilian postal code (CEP).
/// Format: 8 numeric digits, stored without formatting.
/// </summary>
public sealed partial class Cep : ValueObject
{
    private Cep(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// Returns formatted CEP: 00000-000
    /// </summary>
    public string Formatted => $"{Value[..5]}-{Value[5..]}";

    public static Result<Cep> Create(string? cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            return Result.Failure<Cep>("CEP is required.");

        // Remove any non-digit characters
        var digits = DigitsOnly().Replace(cep, "");

        if (digits.Length != 8)
            return Result.Failure<Cep>("CEP must contain exactly 8 digits.");

        if (!IsValidCep().IsMatch(digits))
            return Result.Failure<Cep>("CEP must contain only digits.");

        return new Cep(digits);
    }

    [GeneratedRegex(@"\D")]
    private static partial Regex DigitsOnly();

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex IsValidCep();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;

    public static implicit operator string(Cep cep) => cep.Value;
}