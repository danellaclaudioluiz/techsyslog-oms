using FluentAssertions;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Tests.ValueObjects;

public class AddressTests
{
    private static Cep ValidCep => Cep.Create("01310100").Value;

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Street.Should().Be("Avenida Paulista");
        result.Value.Number.Should().Be("1000");
        result.Value.Neighborhood.Should().Be("Bela Vista");
        result.Value.City.Should().Be("São Paulo");
        result.Value.State.Should().Be("SP");
        result.Value.Complement.Should().BeNull();
    }

    [Fact]
    public void Create_WithComplement_ShouldReturnSuccess()
    {
        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP",
            "Apto 101");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Complement.Should().Be("Apto 101");
    }

    [Fact]
    public void Create_WithNullCep_ShouldReturnFailure()
    {
        // Act
        var result = Address.Create(
            null!,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("CEP is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidStreet_ShouldReturnFailure(string? street)
    {
        // Act
        var result = Address.Create(
            ValidCep,
            street,
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Street is required.");
    }

    [Fact]
    public void Create_WithStreetExceeding200Characters_ShouldReturnFailure()
    {
        // Arrange
        var street = new string('a', 201);

        // Act
        var result = Address.Create(
            ValidCep,
            street,
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Street must not exceed 200 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidNumber_ShouldReturnFailure(string? number)
    {
        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            number,
            "Bela Vista",
            "São Paulo",
            "SP");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Number is required.");
    }

    [Fact]
    public void Create_WithNumberExceeding20Characters_ShouldReturnFailure()
    {
        // Arrange
        var number = new string('1', 21);

        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            number,
            "Bela Vista",
            "São Paulo",
            "SP");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Number must not exceed 20 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidNeighborhood_ShouldReturnFailure(string? neighborhood)
    {
        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            neighborhood,
            "São Paulo",
            "SP");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Neighborhood is required.");
    }

    [Fact]
    public void Create_WithNeighborhoodExceeding100Characters_ShouldReturnFailure()
    {
        // Arrange
        var neighborhood = new string('a', 101);

        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            neighborhood,
            "São Paulo",
            "SP");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Neighborhood must not exceed 100 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCity_ShouldReturnFailure(string? city)
    {
        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            city,
            "SP");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("City is required.");
    }

    [Fact]
    public void Create_WithCityExceeding100Characters_ShouldReturnFailure()
    {
        // Arrange
        var city = new string('a', 101);

        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            city,
            "SP");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("City must not exceed 100 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidState_ShouldReturnFailure(string? state)
    {
        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            state);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("State is required.");
    }

    [Theory]
    [InlineData("S")]
    [InlineData("SPP")]
    [InlineData("São Paulo")]
    public void Create_WithStateNotTwoCharacters_ShouldReturnFailure(string state)
    {
        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            state);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("State must be a 2-letter code (UF).");
    }

    [Fact]
    public void Create_ShouldNormalizeStateToUpperCase()
    {
        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "sp");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be("SP");
    }

    [Fact]
    public void Create_WithComplementExceeding100Characters_ShouldReturnFailure()
    {
        // Arrange
        var complement = new string('a', 101);

        // Act
        var result = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP",
            complement);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Complement must not exceed 100 characters.");
    }

    [Fact]
    public void ToString_WithoutComplement_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP").Value;

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be("Avenida Paulista, 1000, Bela Vista, São Paulo/SP - 01310-100");
    }

    [Fact]
    public void ToString_WithComplement_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP",
            "Apto 101").Value;

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be("Avenida Paulista, 1000 - Apto 101, Bela Vista, São Paulo/SP - 01310-100");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var address1 = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP").Value;

        var address2 = Address.Create(
            Cep.Create("01310100").Value,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP").Value;

        // Act & Assert
        address1.Should().Be(address2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var address1 = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP").Value;

        var address2 = Address.Create(
            ValidCep,
            "Avenida Paulista",
            "2000",
            "Bela Vista",
            "São Paulo",
            "SP").Value;

        // Act & Assert
        address1.Should().NotBe(address2);
    }
}