using TechsysLog.Domain.Common;

namespace TechsysLog.Domain.ValueObjects;

/// <summary>
/// Represents a complete delivery address.
/// Immutable value object containing all address components.
/// </summary>
public sealed class Address : ValueObject
{
    private Address(
        Cep cep,
        string street,
        string number,
        string neighborhood,
        string city,
        string state,
        string? complement = null)
    {
        Cep = cep;
        Street = street;
        Number = number;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        Complement = complement;
    }

    public Cep Cep { get; }
    public string Street { get; }
    public string Number { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }
    public string? Complement { get; }

    public static Result<Address> Create(
        Cep cep,
        string? street,
        string? number,
        string? neighborhood,
        string? city,
        string? state,
        string? complement = null)
    {
        if (cep is null)
            return Result.Failure<Address>("CEP is required.");

        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<Address>("Street is required.");

        if (street.Length > 200)
            return Result.Failure<Address>("Street must not exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(number))
            return Result.Failure<Address>("Number is required.");

        if (number.Length > 20)
            return Result.Failure<Address>("Number must not exceed 20 characters.");

        if (string.IsNullOrWhiteSpace(neighborhood))
            return Result.Failure<Address>("Neighborhood is required.");

        if (neighborhood.Length > 100)
            return Result.Failure<Address>("Neighborhood must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address>("City is required.");

        if (city.Length > 100)
            return Result.Failure<Address>("City must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(state))
            return Result.Failure<Address>("State is required.");

        if (state.Length != 2)
            return Result.Failure<Address>("State must be a 2-letter code (UF).");

        if (complement?.Length > 100)
            return Result.Failure<Address>("Complement must not exceed 100 characters.");

        return new Address(
            cep,
            street.Trim(),
            number.Trim(),
            neighborhood.Trim(),
            city.Trim(),
            state.Trim().ToUpperInvariant(),
            complement?.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Cep;
        yield return Street;
        yield return Number;
        yield return Neighborhood;
        yield return City;
        yield return State;
        yield return Complement;
    }

    public override string ToString()
    {
        var address = $"{Street}, {Number}";
        
        if (!string.IsNullOrWhiteSpace(Complement))
            address += $" - {Complement}";

        address += $", {Neighborhood}, {City}/{State} - {Cep.Formatted}";
        
        return address;
    }
}