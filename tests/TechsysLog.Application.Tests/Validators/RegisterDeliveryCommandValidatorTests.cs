using FluentAssertions;
using TechsysLog.Application.Commands.Deliveries;

namespace TechsysLog.Application.Tests.Validators;

public class RegisterDeliveryCommandValidatorTests
{
    private readonly RegisterDeliveryCommandValidator _validator;

    public RegisterDeliveryCommandValidatorTests()
    {
        _validator = new RegisterDeliveryCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RegisterDeliveryCommand
        {
            OrderId = Guid.NewGuid(),
            DeliveredBy = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyOrderId_ShouldFail()
    {
        // Arrange
        var command = new RegisterDeliveryCommand
        {
            OrderId = Guid.Empty,
            DeliveredBy = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderId");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Order ID is required.");
    }

    [Fact]
    public async Task Validate_WithEmptyDeliveredBy_ShouldFail()
    {
        // Arrange
        var command = new RegisterDeliveryCommand
        {
            OrderId = Guid.NewGuid(),
            DeliveredBy = Guid.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DeliveredBy");
        result.Errors.Should().Contain(e => e.ErrorMessage == "DeliveredBy is required.");
    }

    [Fact]
    public async Task Validate_WithBothFieldsEmpty_ShouldFailWithTwoErrors()
    {
        // Arrange
        var command = new RegisterDeliveryCommand
        {
            OrderId = Guid.Empty,
            DeliveredBy = Guid.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}