using FluentAssertions;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Tests.ValueObjects;

public class PasswordTests
{
    // Simple hash function for testing purposes
    private static string TestHashFunction(string plainText) => $"hashed_{plainText}";

    [Fact]
    public void Create_WithValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        var plainText = "Test@123";

        // Act
        var result = Password.Create(plainText, TestHashFunction);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Hash.Should().Be("hashed_Test@123");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmpty_ShouldReturnFailure(string? plainText)
    {
        // Act
        var result = Password.Create(plainText, TestHashFunction);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password is required.");
    }

    [Fact]
    public void Create_WithLessThan8Characters_ShouldReturnFailure()
    {
        // Arrange
        var plainText = "Te@1abc";

        // Act
        var result = Password.Create(plainText, TestHashFunction);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password must be at least 8 characters.");
    }

    [Fact]
    public void Create_WithMoreThan128Characters_ShouldReturnFailure()
    {
        // Arrange
        var plainText = new string('a', 129) + "T@1";

        // Act
        var result = Password.Create(plainText, TestHashFunction);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password must not exceed 128 characters.");
    }

    [Fact]
    public void Create_WithoutUpperCase_ShouldReturnFailure()
    {
        // Arrange
        var plainText = "test@123";

        // Act
        var result = Password.Create(plainText, TestHashFunction);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Create_WithoutLowerCase_ShouldReturnFailure()
    {
        // Arrange
        var plainText = "TEST@123";

        // Act
        var result = Password.Create(plainText, TestHashFunction);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void Create_WithoutDigit_ShouldReturnFailure()
    {
        // Arrange
        var plainText = "Test@abc";

        // Act
        var result = Password.Create(plainText, TestHashFunction);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password must contain at least one digit.");
    }

    [Fact]
    public void Create_WithoutSpecialCharacter_ShouldReturnFailure()
    {
        // Arrange
        var plainText = "Test1234";

        // Act
        var result = Password.Create(plainText, TestHashFunction);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password must contain at least one special character.");
    }

    [Fact]
    public void FromHash_WithValidHash_ShouldReturnPassword()
    {
        // Arrange
        var hash = "existing_hash_from_database";

        // Act
        var password = Password.FromHash(hash);

        // Assert
        password.Hash.Should().Be(hash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromHash_WithNullOrEmpty_ShouldThrowException(string? hash)
    {
        // Act
        var act = () => Password.FromHash(hash!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Hash cannot be empty.*");
    }

    [Fact]
    public void Equals_WithSameHash_ShouldReturnTrue()
    {
        // Arrange
        var password1 = Password.Create("Test@123", TestHashFunction).Value;
        var password2 = Password.Create("Test@123", TestHashFunction).Value;

        // Act & Assert
        password1.Should().Be(password2);
    }

    [Fact]
    public void Equals_WithDifferentHash_ShouldReturnFalse()
    {
        // Arrange
        var password1 = Password.Create("Test@123", TestHashFunction).Value;
        var password2 = Password.Create("Test@456", TestHashFunction).Value;

        // Act & Assert
        password1.Should().NotBe(password2);
    }
}