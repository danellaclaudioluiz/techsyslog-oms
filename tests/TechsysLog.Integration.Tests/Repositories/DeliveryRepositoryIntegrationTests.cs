using FluentAssertions;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.ValueObjects;
using TechsysLog.Infrastructure.Persistence.Repositories;
using TechsysLog.Integration.Tests.Fixtures;

namespace TechsysLog.Integration.Tests.Repositories;

public class DeliveryRepositoryIntegrationTests : IClassFixture<MongoDbFixture>
{
    private readonly DeliveryRepository _repository;

    public DeliveryRepositoryIntegrationTests(MongoDbFixture fixture)
    {
        _repository = new DeliveryRepository(fixture.Context);
    }

    private static (Order order, Delivery delivery) CreateTestDelivery(Guid? deliveredBy = null)
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
            OrderNumber.Generate(Random.Shared.Next(1, 99999)),
            "Test order",
            100m,
            address,
            Guid.NewGuid()).Value;

        order.Confirm();
        order.StartDelivery();
        order.ClearDomainEvents();

        var delivery = Delivery.Create(order, deliveredBy ?? Guid.NewGuid()).Value;
        delivery.ClearDomainEvents();

        return (order, delivery);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistDelivery()
    {
        // Arrange
        var (_, delivery) = CreateTestDelivery();

        // Act
        var result = await _repository.AddAsync(delivery);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(delivery.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingDelivery_ShouldReturnDelivery()
    {
        // Arrange
        var (order, delivery) = CreateTestDelivery();
        await _repository.AddAsync(delivery);

        // Act
        var result = await _repository.GetByIdAsync(delivery.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(delivery.Id);
        result.OrderId.Should().Be(order.Id);
        result.DeliveredBy.Should().Be(delivery.DeliveredBy);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingDelivery_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOrderIdAsync_WithExistingDelivery_ShouldReturnDelivery()
    {
        // Arrange
        var (order, delivery) = CreateTestDelivery();
        await _repository.AddAsync(delivery);

        // Act
        var result = await _repository.GetByOrderIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetByOrderIdAsync_WithNonExistingDelivery_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByOrderIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByDeliveredByAsync_ShouldReturnDeliveriesForUser()
    {
        // Arrange
        var deliveredBy = Guid.NewGuid();
        var (_, delivery1) = CreateTestDelivery(deliveredBy);
        var (_, delivery2) = CreateTestDelivery(deliveredBy);
        var (_, delivery3) = CreateTestDelivery(Guid.NewGuid());
        await _repository.AddAsync(delivery1);
        await _repository.AddAsync(delivery2);
        await _repository.AddAsync(delivery3);

        // Act
        var result = await _repository.GetByDeliveredByAsync(deliveredBy);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Id == delivery1.Id);
        result.Should().Contain(d => d.Id == delivery2.Id);
        result.Should().NotContain(d => d.Id == delivery3.Id);
    }

    [Fact]
    public async Task OrderHasDeliveryAsync_WithExistingDelivery_ShouldReturnTrue()
    {
        // Arrange
        var (order, delivery) = CreateTestDelivery();
        await _repository.AddAsync(delivery);

        // Act
        var result = await _repository.OrderHasDeliveryAsync(order.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task OrderHasDeliveryAsync_WithNonExistingDelivery_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.OrderHasDeliveryAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var (_, delivery) = CreateTestDelivery();
        await _repository.AddAsync(delivery);

        // Act
        await _repository.UpdateAsync(delivery);
        var result = await _repository.GetByIdAsync(delivery.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteDelivery()
    {
        // Arrange
        var (_, delivery) = CreateTestDelivery();
        await _repository.AddAsync(delivery);

        // Act
        await _repository.DeleteAsync(delivery);
        var result = await _repository.GetByIdAsync(delivery.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyNonDeletedDeliveries()
    {
        // Arrange
        var (_, delivery1) = CreateTestDelivery();
        var (_, delivery2) = CreateTestDelivery();
        await _repository.AddAsync(delivery1);
        await _repository.AddAsync(delivery2);
        await _repository.DeleteAsync(delivery1);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().Contain(d => d.Id == delivery2.Id);
        result.Should().NotContain(d => d.Id == delivery1.Id);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var deliveredBy = Guid.NewGuid();
        var (_, delivery1) = CreateTestDelivery(deliveredBy);
        var (_, delivery2) = CreateTestDelivery(deliveredBy);
        await _repository.AddAsync(delivery1);
        await _repository.AddAsync(delivery2);

        // Act
        var result = await _repository.CountAsync(d => d.DeliveredBy == deliveredBy);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task ExistsAsync_WithMatchingPredicate_ShouldReturnTrue()
    {
        // Arrange
        var (order, delivery) = CreateTestDelivery();
        await _repository.AddAsync(delivery);

        // Act
        var result = await _repository.ExistsAsync(d => d.OrderId == order.Id);

        // Assert
        result.Should().BeTrue();
    }
}