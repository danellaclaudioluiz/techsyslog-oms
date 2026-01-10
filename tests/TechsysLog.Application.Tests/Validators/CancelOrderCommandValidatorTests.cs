using FluentAssertions;
using TechsysLog.Application.Commands.Orders;

namespace TechsysLog.Application.Tests.Validators;

public class CancelOrderCommandValidatorTests
{
    private readonly CancelOrderCommandValidator _validator;

    public CancelOrderCommandValidatorTests()
    {
        _validator = new CancelOrderCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CancelOrderCommand
        {
            OrderId = Guid.NewGuid()
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
        var command = new CancelOrderCommand
        {
            OrderId = Guid.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderId");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Order ID is required.");
    }
}