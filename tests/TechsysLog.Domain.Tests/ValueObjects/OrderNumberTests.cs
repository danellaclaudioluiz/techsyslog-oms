using FluentAssertions;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Tests.ValueObjects;

public class OrderNumberTests
{
    [Fact]
    public void Generate_WithValidSequence_ShouldReturnOrderNumber()
    {
        // Arrange
        var sequence = 1;

        // Act
        var orderNumber = OrderNumber.Generate(sequence);

        // Assert
        orderNumber.Value.Should().MatchRegex(@"^ORD-\d{8}-00001$");
    }

    [Fact]
    public void Generate_WithSequence99999_ShouldReturnOrderNumber()
    {
        // Arrange
        var sequence = 99999;

        // Act
        var orderNumber = OrderNumber.Generate(sequence);

        // Assert
        orderNumber.Value.Should().MatchRegex(@"^ORD-\d{8}-99999$");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100000)]
    public void Generate_WithInvalidSequence_ShouldThrowException(int sequence)
    {
        // Act
        var act = () => OrderNumber.Generate(sequence);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Sequence must be between 1 and 99999*");
    }

    [Fact]
    public void Create_WithValidOrderNumber_ShouldReturnSuccess()
    {
        // Arrange
        var value = "ORD-20240115-00001";

        // Act
        var result = OrderNumber.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("ORD-20240115-00001");
    }

    [Fact]
    public void Create_WithLowerCase_ShouldNormalizeToUpperCase()
    {
        // Arrange
        var value = "ord-20240115-00001";

        // Act
        var result = OrderNumber.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("ORD-20240115-00001");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmpty_ShouldReturnFailure(string? value)
    {
        // Act
        var result = OrderNumber.Create(value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order number is required.");
    }

    [Theory]
    [InlineData("ORD-2024011-00001")]
    [InlineData("ORD-202401150-00001")]
    [InlineData("ORD-20240115-0001")]
    [InlineData("ORD-20240115-000001")]
    [InlineData("ORDER-20240115-00001")]
    [InlineData("20240115-00001")]
    [InlineData("ORD2024011500001")]
    public void Create_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        // Act
        var result = OrderNumber.Create(value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order number format is invalid. Expected: ORD-YYYYMMDD-XXXXX");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var orderNumber = OrderNumber.Create("ORD-20240115-00001").Value;

        // Act
        var result = orderNumber.ToString();

        // Assert
        result.Should().Be("ORD-20240115-00001");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var orderNumber = OrderNumber.Create("ORD-20240115-00001").Value;

        // Act
        string value = orderNumber;

        // Assert
        value.Should().Be("ORD-20240115-00001");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var orderNumber1 = OrderNumber.Create("ORD-20240115-00001").Value;
        var orderNumber2 = OrderNumber.Create("ORD-20240115-00001").Value;

        // Act & Assert
        orderNumber1.Should().Be(orderNumber2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var orderNumber1 = OrderNumber.Create("ORD-20240115-00001").Value;
        var orderNumber2 = OrderNumber.Create("ORD-20240115-00002").Value;

        // Act & Assert
        orderNumber1.Should().NotBe(orderNumber2);
    }
}