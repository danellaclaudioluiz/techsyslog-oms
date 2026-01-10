using FluentAssertions;
using TechsysLog.Application.Commands.Orders;

namespace TechsysLog.Application.Tests.Validators;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order description",
            Value = 100.50m,
            Cep = "01310100",
            Number = "1000",
            Complement = "Apto 101",
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithFormattedCep_ShouldPass()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = "01310-100",
            Number = "1000",
            UserId = Guid.NewGuid()
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
    public async Task Validate_WithInvalidDescription_ShouldFail(string? description)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = description!,
            Value = 100m,
            Cep = "01310100",
            Number = "1000",
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task Validate_WithDescriptionExceeding500Characters_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = new string('a', 501),
            Value = 100m,
            Cep = "01310100",
            Number = "1000",
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("500"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public async Task Validate_WithInvalidValue_ShouldFail(decimal value)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = value,
            Cep = "01310100",
            Number = "1000",
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Value");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("abcdefgh")]
    public async Task Validate_WithInvalidCep_ShouldFail(string? cep)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = cep!,
            Number = "1000",
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cep");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithInvalidNumber_ShouldFail(string? number)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = "01310100",
            Number = number!,
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Number");
    }

    [Fact]
    public async Task Validate_WithNumberExceeding20Characters_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = "01310100",
            Number = new string('1', 21),
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("20"));
    }

    [Fact]
    public async Task Validate_WithComplementExceeding100Characters_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = "01310100",
            Number = "1000",
            Complement = new string('a', 101),
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = "01310100",
            Number = "1000",
            UserId = Guid.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public async Task Validate_WithNullComplement_ShouldPass()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Description = "Test order",
            Value = 100m,
            Cep = "01310100",
            Number = "1000",
            Complement = null,
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}