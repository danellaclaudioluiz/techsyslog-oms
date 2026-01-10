using FluentAssertions;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Tests.ValueObjects;

public class CepTests
{
    [Fact]
    public void Create_WithValidCep_ShouldReturnSuccess()
    {
        // Arrange
        var cep = "01310100";

        // Act
        var result = Cep.Create(cep);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("01310100");
    }

    [Fact]
    public void Create_WithFormattedCep_ShouldRemoveFormatting()
    {
        // Arrange
        var cep = "01310-100";

        // Act
        var result = Cep.Create(cep);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("01310100");
    }

    [Fact]
    public void Create_WithCepContainingSpaces_ShouldRemoveSpaces()
    {
        // Arrange
        var cep = "01 310 100";

        // Act
        var result = Cep.Create(cep);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("01310100");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmpty_ShouldReturnFailure(string? cep)
    {
        // Act
        var result = Cep.Create(cep);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("CEP is required.");
    }

    [Theory]
    [InlineData("0131010")]
    [InlineData("013101001")]
    [InlineData("123")]
    public void Create_WithInvalidLength_ShouldReturnFailure(string cep)
    {
        // Act
        var result = Cep.Create(cep);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("CEP must contain exactly 8 digits.");
    }

    [Fact]
    public void Formatted_ShouldReturnCepWithHyphen()
    {
        // Arrange
        var cep = Cep.Create("01310100").Value;

        // Act
        var formatted = cep.Formatted;

        // Assert
        formatted.Should().Be("01310-100");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedCep()
    {
        // Arrange
        var cep = Cep.Create("01310100").Value;

        // Act
        var result = cep.ToString();

        // Assert
        result.Should().Be("01310-100");
    }

    [Fact]
    public void Equals_WithSameCep_ShouldReturnTrue()
    {
        // Arrange
        var cep1 = Cep.Create("01310100").Value;
        var cep2 = Cep.Create("01310-100").Value;

        // Act & Assert
        cep1.Should().Be(cep2);
    }

    [Fact]
    public void Equals_WithDifferentCep_ShouldReturnFalse()
    {
        // Arrange
        var cep1 = Cep.Create("01310100").Value;
        var cep2 = Cep.Create("01310200").Value;

        // Act & Assert
        cep1.Should().NotBe(cep2);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var cep = Cep.Create("01310100").Value;

        // Act
        string value = cep;

        // Assert
        value.Should().Be("01310100");
    }
}