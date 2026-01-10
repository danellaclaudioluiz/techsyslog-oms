using FluentAssertions;
using TechsysLog.Infrastructure.Services;

namespace TechsysLog.Infrastructure.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void Hash_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "Test@123";

        // Act
        var hash = _passwordHasher.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        hash.Should().StartWith("$2"); // BCrypt hash prefix
    }

    [Fact]
    public void Hash_ShouldReturnDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "Test@123";

        // Act
        var hash1 = _passwordHasher.Hash(password);
        var hash2 = _passwordHasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2); // BCrypt uses random salt
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "Test@123";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "Test@123";
        var wrongPassword = "Wrong@123";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "Test@123";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(string.Empty, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("Complex@Password123!")]
    [InlineData("12345678")]
    [InlineData("with spaces in password")]
    public void Hash_AndVerify_ShouldWorkWithVariousPasswords(string password)
    {
        // Act
        var hash = _passwordHasher.Hash(password);
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }
}