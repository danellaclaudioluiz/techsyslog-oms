using FluentAssertions;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Tests.Entities;

public class UserTests
{
    private static Email ValidEmail => Email.Create("test@example.com").Value;
    private static Password ValidPassword => Password.Create("Test@123", s => $"hashed_{s}").Value;

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Act
        var result = User.Create("John Doe", ValidEmail, ValidPassword);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("John Doe");
        result.Value.Email.Should().Be(ValidEmail);
        result.Value.Role.Should().Be(UserRole.Customer);
    }

    [Fact]
    public void Create_WithSpecificRole_ShouldReturnSuccess()
    {
        // Act
        var result = User.Create("John Doe", ValidEmail, ValidPassword, UserRole.Admin);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(UserRole.Admin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldReturnFailure(string? name)
    {
        // Act
        var result = User.Create(name, ValidEmail, ValidPassword);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Name is required.");
    }

    [Fact]
    public void Create_WithNameExceeding150Characters_ShouldReturnFailure()
    {
        // Arrange
        var name = new string('a', 151);

        // Act
        var result = User.Create(name, ValidEmail, ValidPassword);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Name must not exceed 150 characters.");
    }

    [Fact]
    public void Create_WithNullEmail_ShouldReturnFailure()
    {
        // Act
        var result = User.Create("John Doe", null!, ValidPassword);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email is required.");
    }

    [Fact]
    public void Create_WithNullPassword_ShouldReturnFailure()
    {
        // Act
        var result = User.Create("John Doe", ValidEmail, null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password is required.");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldReturnSuccess()
    {
        // Arrange
        var user = User.Create("John Doe", ValidEmail, ValidPassword).Value;

        // Act
        var result = user.UpdateName("Jane Doe");

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Name.Should().Be("Jane Doe");
        user.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithInvalidName_ShouldReturnFailure(string? name)
    {
        // Arrange
        var user = User.Create("John Doe", ValidEmail, ValidPassword).Value;

        // Act
        var result = user.UpdateName(name);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Name is required.");
    }

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        var user = User.Create("John Doe", ValidEmail, ValidPassword).Value;
        var newEmail = Email.Create("new@example.com").Value;

        // Act
        var result = user.UpdateEmail(newEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Email.Should().Be(newEmail);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateEmail_WithNullEmail_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Create("John Doe", ValidEmail, ValidPassword).Value;

        // Act
        var result = user.UpdateEmail(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email is required.");
    }

    [Fact]
    public void UpdatePassword_WithValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        var user = User.Create("John Doe", ValidEmail, ValidPassword).Value;
        var newPassword = Password.Create("NewPass@123", s => $"hashed_{s}").Value;

        // Act
        var result = user.UpdatePassword(newPassword);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Password.Should().Be(newPassword);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePassword_WithNullPassword_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Create("John Doe", ValidEmail, ValidPassword).Value;

        // Act
        var result = user.UpdatePassword(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password is required.");
    }

    [Fact]
    public void ChangeRole_WithDifferentRole_ShouldReturnSuccess()
    {
        // Arrange
        var user = User.Create("John Doe", ValidEmail, ValidPassword).Value;

        // Act
        var result = user.ChangeRole(UserRole.Operator);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Role.Should().Be(UserRole.Operator);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ChangeRole_WithSameRole_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Create("John Doe", ValidEmail, ValidPassword).Value;

        // Act
        var result = user.ChangeRole(UserRole.Customer);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User already has this role.");
    }

    [Theory]
    [InlineData(UserRole.Admin, UserRole.Customer, true)]
    [InlineData(UserRole.Admin, UserRole.Operator, true)]
    [InlineData(UserRole.Admin, UserRole.Admin, true)]
    [InlineData(UserRole.Operator, UserRole.Customer, true)]
    [InlineData(UserRole.Operator, UserRole.Operator, true)]
    [InlineData(UserRole.Operator, UserRole.Admin, false)]
    [InlineData(UserRole.Customer, UserRole.Customer, true)]
    [InlineData(UserRole.Customer, UserRole.Operator, false)]
    [InlineData(UserRole.Customer, UserRole.Admin, false)]
    public void HasPermission_ShouldReturnExpectedResult(
        UserRole userRole,
        UserRole requiredRole,
        bool expected)
    {
        // Arrange
        var user = User.Create("John Doe", ValidEmail, ValidPassword, userRole).Value;

        // Act
        var result = user.HasPermission(requiredRole);

        // Assert
        result.Should().Be(expected);
    }
}