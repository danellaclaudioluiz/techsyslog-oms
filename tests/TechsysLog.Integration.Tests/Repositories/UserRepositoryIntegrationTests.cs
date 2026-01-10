using FluentAssertions;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.ValueObjects;
using TechsysLog.Infrastructure.Persistence.Repositories;
using TechsysLog.Integration.Tests.Fixtures;

namespace TechsysLog.Integration.Tests.Repositories;

public class UserRepositoryIntegrationTests : IClassFixture<MongoDbFixture>
{
    private readonly UserRepository _repository;

    public UserRepositoryIntegrationTests(MongoDbFixture fixture)
    {
        _repository = new UserRepository(fixture.Context);
    }

    private static User CreateTestUser(string email = "test@example.com", string name = "Test User")
    {
        var emailVo = Email.Create(email).Value;
        var password = Password.FromHash("hashed_password");
        return User.Create(name, emailVo, password, UserRole.Customer).Value;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistUser()
    {
        // Arrange
        var user = CreateTestUser($"add_{Guid.NewGuid()}@example.com");

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = CreateTestUser($"getbyid_{Guid.NewGuid()}@example.com");
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Name.Should().Be(user.Name);
        result.Email.Value.Should().Be(user.Email.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingUser_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var email = $"getbyemail_{Guid.NewGuid()}@example.com";
        var user = CreateTestUser(email);
        await _repository.AddAsync(user);

        var emailVo = Email.Create(email).Value;

        // Act
        var result = await _repository.GetByEmailAsync(emailVo);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Value.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var emailVo = Email.Create("nonexistent@example.com").Value;

        // Act
        var result = await _repository.GetByEmailAsync(emailVo);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = $"emailexists_{Guid.NewGuid()}@example.com";
        var user = CreateTestUser(email);
        await _repository.AddAsync(user);

        var emailVo = Email.Create(email).Value;

        // Act
        var result = await _repository.EmailExistsAsync(emailVo);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WithNonExistingEmail_ShouldReturnFalse()
    {
        // Arrange
        var emailVo = Email.Create($"notexists_{Guid.NewGuid()}@example.com").Value;

        // Act
        var result = await _repository.EmailExistsAsync(emailVo);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var user = CreateTestUser($"update_{Guid.NewGuid()}@example.com");
        await _repository.AddAsync(user);

        user.UpdateName("Updated Name");

        // Act
        await _repository.UpdateAsync(user);
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteUser()
    {
        // Arrange
        var user = CreateTestUser($"delete_{Guid.NewGuid()}@example.com");
        await _repository.AddAsync(user);

        // Act
        await _repository.DeleteAsync(user);
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().BeNull(); // Soft deleted users are filtered out
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyNonDeletedUsers()
    {
        // Arrange
        var user1 = CreateTestUser($"getall1_{Guid.NewGuid()}@example.com");
        var user2 = CreateTestUser($"getall2_{Guid.NewGuid()}@example.com");
        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _repository.DeleteAsync(user1);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().Contain(u => u.Id == user2.Id);
        result.Should().NotContain(u => u.Id == user1.Id);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var uniquePrefix = Guid.NewGuid().ToString();
        var user1 = CreateTestUser($"count1_{uniquePrefix}@example.com", $"Count User {uniquePrefix}");
        var user2 = CreateTestUser($"count2_{uniquePrefix}@example.com", $"Count User {uniquePrefix}");
        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);

        // Act
        var result = await _repository.CountAsync(u => u.Name.Contains(uniquePrefix));

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task ExistsAsync_WithMatchingPredicate_ShouldReturnTrue()
    {
        // Arrange
        var uniqueName = $"Exists User {Guid.NewGuid()}";
        var user = CreateTestUser($"exists_{Guid.NewGuid()}@example.com", uniqueName);
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.ExistsAsync(u => u.Name == uniqueName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FindAsync_WithPredicate_ShouldReturnMatchingUsers()
    {
        // Arrange
        var uniquePrefix = Guid.NewGuid().ToString();
        var user1 = CreateTestUser($"find1_{uniquePrefix}@example.com", $"Find User {uniquePrefix}");
        var user2 = CreateTestUser($"find2_{uniquePrefix}@example.com", $"Find User {uniquePrefix}");
        var user3 = CreateTestUser($"find3_{uniquePrefix}@example.com", "Other User");
        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _repository.AddAsync(user3);

        // Act
        var result = await _repository.FindAsync(u => u.Name.Contains($"Find User {uniquePrefix}"));

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Id == user1.Id);
        result.Should().Contain(u => u.Id == user2.Id);
    }
}