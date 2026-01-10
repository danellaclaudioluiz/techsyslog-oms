namespace TechsysLog.Application.DTOs;

/// <summary>
/// Data transfer object for Address value object.
/// </summary>
public sealed record AddressDto
{
    public string Cep { get; init; } = null!;
    public string CepFormatted { get; init; } = null!;
    public string Street { get; init; } = null!;
    public string Number { get; init; } = null!;
    public string Neighborhood { get; init; } = null!;
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string? Complement { get; init; }
    public string FullAddress { get; init; } = null!;
}