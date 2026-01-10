using System.Text.RegularExpressions;
using TechsysLog.Domain.Common;

namespace TechsysLog.Domain.ValueObjects;

/// <summary>
/// Represents a unique order identifier.
/// Format: ORD-YYYYMMDD-XXXXX (e.g., ORD-20240115-00001)
/// </summary>
public sealed partial class OrderNumber : ValueObject
{
    private const string Prefix = "ORD";

    private OrderNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// Generates a new order number based on current date and sequence.
    /// </summary>
    public static OrderNumber Generate(int dailySequence)
    {
        if (dailySequence < 1 || dailySequence > 99999)
            throw new ArgumentOutOfRangeException(nameof(dailySequence), "Sequence must be between 1 and 99999.");

        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var sequence = dailySequence.ToString("D5");
        var value = $"{Prefix}-{date}-{sequence}";

        return new OrderNumber(value);
    }

    /// <summary>
    /// Creates an order number from an existing value (e.g., from database).
    /// </summary>
    public static Result<OrderNumber> Create(string? orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return Result.Failure<OrderNumber>("Order number is required.");

        var trimmed = orderNumber.Trim().ToUpperInvariant();

        if (!IsValidFormat().IsMatch(trimmed))
            return Result.Failure<OrderNumber>("Order number format is invalid. Expected: ORD-YYYYMMDD-XXXXX");

        return new OrderNumber(trimmed);
    }

    [GeneratedRegex(@"^ORD-\d{8}-\d{5}$")]
    private static partial Regex IsValidFormat();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(OrderNumber orderNumber) => orderNumber.Value;
}
