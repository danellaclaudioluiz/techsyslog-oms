using FluentAssertions;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        var emailAddress = "test@example.com";

        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithValidEmail_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var emailAddress = "Test@EXAMPLE.com";

        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmpty_ShouldReturnFailure(string? email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email is required.");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("test@.com")]
    [InlineData("test@example")]
    [InlineData("test @example.com")]
    public void Create_WithInvalidFormat_ShouldReturnFailure(string email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email format is invalid.");
    }

    [Fact]
    public void Create_WithEmailExceeding256Characters_ShouldReturnFailure()
    {
        // Arrange
        var email = new string('a', 250) + "@example.com";

        // Act
        var result = Email.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email must not exceed 256 characters.");
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com").Value;
        var email2 = Email.Create("test@example.com").Value;

        // Act & Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com").Value;
        var email2 = Email.Create("test2@example.com").Value;

        // Act & Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;

        // Act
        string value = email;

        // Assert
        value.Should().Be("test@example.com");
    }
}