using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechsysLog.API.Controllers;
using TechsysLog.API.Models;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Queries.Users;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.API.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UsersController _controller;
    private readonly Guid _adminId;

    public UsersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _controller = new UsersController(_mediatorMock.Object, _userRepositoryMock.Object);
        _adminId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _adminId.ToString()),
            new(ClaimTypes.Email, "admin@test.com"),
            new(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    private static User CreateTestUser(Guid? id = null, string email = "user@test.com")
    {
        var emailVo = Email.Create(email).Value;
        var password = Password.FromHash("hashed-password");
        var user = User.Create("Test User", emailVo, password, UserRole.Customer).Value;
        return user;
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(email: "user1@test.com"),
            CreateTestUser(email: "user2@test.com")
        };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<UserDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<UserDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDto
        {
            Id = userId,
            Name = "Test User",
            Email = "user@test.com",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetById(userId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetById_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetById(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WithExistingUser_ShouldReturnSuccess()
    {
        // Arrange
        var user = CreateTestUser();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.DeleteAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(user.Id, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Delete(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldCallRepositoryDeleteAsync()
    {
        // Arrange
        var user = CreateTestUser();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _controller.Delete(user.Id, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.DeleteAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnUpdatedUser()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new UpdateUserRequest
        {
            Name = "Updated Name",
            Role = "Operator"
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(user.Id, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Update_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest { Name = "New Name" };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Update(userId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WithInvalidRole_ShouldReturnBadRequest()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new UpdateUserRequest { Role = "InvalidRole" };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Update(user.Id, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithOnlyName_ShouldUpdateOnlyName()
    {
        // Arrange
        var user = CreateTestUser();
        var originalRole = user.Role;
        var request = new UpdateUserRequest { Name = "Only Name Update" };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(user.Id, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
        response.Data!.Name.Should().Be("Only Name Update");
        response.Data.Role.Should().Be(originalRole);
    }
}