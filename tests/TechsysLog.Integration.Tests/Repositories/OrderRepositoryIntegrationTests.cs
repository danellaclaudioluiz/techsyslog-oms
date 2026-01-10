using FluentAssertions;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.ValueObjects;
using TechsysLog.Infrastructure.Persistence.Repositories;
using TechsysLog.Integration.Tests.Fixtures;

namespace TechsysLog.Integration.Tests.Repositories;

public class OrderRepositoryIntegrationTests : IClassFixture<MongoDbFixture>
{
    private readonly OrderRepository _repository;

    public OrderRepositoryIntegrationTests(MongoDbFixture fixture)
    {
        _repository = new OrderRepository(fixture.Context);
    }

    private static Order CreateTestOrder(int sequence = 1, Guid? userId = null)
    {
        var cep = Cep.Create("01310100").Value;
        var address = Address.Create(
            cep,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP").Value;

        var order = Order.Create(
            OrderNumber.Generate(sequence),
            $"Test order {sequence}",
            100m * sequence,
            address,
            userId ?? Guid.NewGuid()).Value;

        order.ClearDomainEvents();
        return order;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistOrder()
    {
        // Arrange
        var order = CreateTestOrder(Random.Shared.Next(1, 99999));

        // Act
        var result = await _repository.AddAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        var order = CreateTestOrder(Random.Shared.Next(1, 99999));
        await _repository.AddAsync(order);

        // Act
        var result = await _repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Description.Should().Be(order.Description);
        result.Value.Should().Be(order.Value);
        result.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingOrder_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOrderNumberAsync_WithExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        var order = CreateTestOrder(Random.Shared.Next(1, 99999));
        await _repository.AddAsync(order);

        // Act
        var result = await _repository.GetByOrderNumberAsync(order.OrderNumber);

        // Assert
        result.Should().NotBeNull();
        result!.OrderNumber.Value.Should().Be(order.OrderNumber.Value);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var order1 = CreateTestOrder(Random.Shared.Next(1, 99999), userId);
        var order2 = CreateTestOrder(Random.Shared.Next(1, 99999), userId);
        var order3 = CreateTestOrder(Random.Shared.Next(1, 99999), Guid.NewGuid());
        await _repository.AddAsync(order1);
        await _repository.AddAsync(order2);
        await _repository.AddAsync(order3);

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == order1.Id);
        result.Should().Contain(o => o.Id == order2.Id);
        result.Should().NotContain(o => o.Id == order3.Id);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnOrdersWithStatus()
    {
        // Arrange
        var order1 = CreateTestOrder(Random.Shared.Next(1, 99999));
        var order2 = CreateTestOrder(Random.Shared.Next(1, 99999));
        var order3 = CreateTestOrder(Random.Shared.Next(1, 99999));
        order1.Confirm();
        order2.Confirm();
        await _repository.AddAsync(order1);
        await _repository.AddAsync(order2);
        await _repository.AddAsync(order3);

        // Act
        var result = await _repository.GetByStatusAsync(OrderStatus.Confirmed);

        // Assert
        result.Should().Contain(o => o.Id == order1.Id);
        result.Should().Contain(o => o.Id == order2.Id);
        result.Should().NotContain(o => o.Id == order3.Id);
    }

    [Fact]
    public async Task GetDailyOrderCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var order1 = CreateTestOrder(Random.Shared.Next(1, 99999));
        var order2 = CreateTestOrder(Random.Shared.Next(1, 99999));
        await _repository.AddAsync(order1);
        await _repository.AddAsync(order2);

        // Act
        var result = await _repository.GetDailyOrderCountAsync(today);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var order = CreateTestOrder(Random.Shared.Next(1, 99999));
        await _repository.AddAsync(order);

        order.Confirm();

        // Act
        await _repository.UpdateAsync(order);
        var result = await _repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(OrderStatus.Confirmed);
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteOrder()
    {
        // Arrange
        var order = CreateTestOrder(Random.Shared.Next(1, 99999));
        await _repository.AddAsync(order);

        // Act
        await _repository.DeleteAsync(order);
        var result = await _repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOrdersSortedByCreatedAtDescending()
    {
        // Arrange
        var order1 = CreateTestOrder(Random.Shared.Next(1, 99999));
        await _repository.AddAsync(order1);
        await Task.Delay(10);
        var order2 = CreateTestOrder(Random.Shared.Next(1, 99999));
        await _repository.AddAsync(order2);

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        var index1 = result.FindIndex(o => o.Id == order1.Id);
        var index2 = result.FindIndex(o => o.Id == order2.Id);
        index2.Should().BeLessThan(index1); // order2 should come first (newer)
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var order1 = CreateTestOrder(Random.Shared.Next(1, 99999), userId);
        var order2 = CreateTestOrder(Random.Shared.Next(1, 99999), userId);
        var order3 = CreateTestOrder(Random.Shared.Next(1, 99999), Guid.NewGuid());
        await _repository.AddAsync(order1);
        await _repository.AddAsync(order2);
        await _repository.AddAsync(order3);

        // Act
        var result = await _repository.CountAsync(o => o.UserId == userId);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task ExistsAsync_WithMatchingPredicate_ShouldReturnTrue()
    {
        // Arrange
        var order = CreateTestOrder(Random.Shared.Next(1, 99999));
        await _repository.AddAsync(order);

        // Act
        var result = await _repository.ExistsAsync(o => o.Id == order.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FindAsync_WithPredicate_ShouldReturnMatchingOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var order1 = CreateTestOrder(Random.Shared.Next(1, 99999), userId);
        var order2 = CreateTestOrder(Random.Shared.Next(1, 99999), userId);
        order1.Confirm();
        await _repository.AddAsync(order1);
        await _repository.AddAsync(order2);

        // Act
        var result = await _repository.FindAsync(o => o.UserId == userId && o.Status == OrderStatus.Confirmed);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(o => o.Id == order1.Id);
    }
}