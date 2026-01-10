using FluentAssertions;
using TechsysLog.Application.Commands.Users;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.Tests.Validators;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        _validator = new CreateUserCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Test@123",
            Role = UserRole.Customer
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithInvalidName_ShouldFail(string? name)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = name!,
            Email = "john@example.com",
            Password = "Test@123",
            Role = UserRole.Customer
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithNameExceeding150Characters_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = new string('a', 151),
            Email = "john@example.com",
            Password = "Test@123",
            Role = UserRole.Customer
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("150"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    public async Task Validate_WithInvalidEmail_ShouldFail(string? email)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = email!,
            Password = "Test@123",
            Role = UserRole.Customer
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WithEmptyPassword_ShouldFail(string? password)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = password!,
            Role = UserRole.Customer
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task Validate_WithShortPassword_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Te@1",
            Role = UserRole.Customer
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("8"));
    }

    [Fact]
    public async Task Validate_WithPasswordMissingUppercase_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "test@123",
            Role = UserRole.Customer
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("uppercase"));
    }

    [Fact]
    public async Task Validate_WithPasswordMissingLowercase_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "TEST@123",
            Role = UserRole.Customer
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("lowercase"));
    }

    [Fact]
    public async Task Validate_WithPasswordMissingDigit_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Test@abc",
            Role = UserRole.Customer
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("digit"));
    }

    [Fact]
    public async Task Validate_WithPasswordMissingSpecialCharacter_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Test1234",
            Role = UserRole.Customer
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("special"));
    }

    [Fact]
    public async Task Validate_WithInvalidRole_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Test@123",
            Role = (UserRole)999
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Role");
    }
}