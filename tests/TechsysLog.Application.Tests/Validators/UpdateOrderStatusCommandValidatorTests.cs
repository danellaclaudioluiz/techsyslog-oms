using FluentAssertions;
using TechsysLog.Application.Commands.Orders;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.Tests.Validators;

public class UpdateOrderStatusCommandValidatorTests
{
    private readonly UpdateOrderStatusCommandValidator _validator;

    public UpdateOrderStatusCommandValidatorTests()
    {
        _validator = new UpdateOrderStatusCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            NewStatus = OrderStatus.Confirmed
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
        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.Empty,
            NewStatus = OrderStatus.Confirmed
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderId");
    }

    [Fact]
    public async Task Validate_WithInvalidStatus_ShouldFail()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            NewStatus = (OrderStatus)999
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewStatus");
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.InTransit)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Validate_WithAllValidStatuses_ShouldPass(OrderStatus status)
    {
        // Arrange
        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            NewStatus = status
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}