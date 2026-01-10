using AutoMapper;
using FluentAssertions;
using NSubstitute;
using TechsysLog.Application.Commands.Users;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Mappings;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Tests.Commands.Users;

public class CreateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateUserCommandHandler(
            _userRepository,
            _passwordHasher,
            _mapper);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithUserDto()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Test@123",
            Role = UserRole.Customer
        };

        _userRepository.EmailExistsAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordHasher.Hash(Arg.Any<string>())
            .Returns("hashed_password");

        _userRepository.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<User>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("John Doe");
        result.Value.Email.Should().Be("john@example.com");
        result.Value.Role.Should().Be(UserRole.Customer);

        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "existing@example.com",
            Password = "Test@123",
            Role = UserRole.Customer
        };

        _userRepository.EmailExistsAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email is already registered.");

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "invalid-email",
            Password = "Test@123",
            Role = UserRole.Customer
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email format is invalid.");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "weak",
            Role = UserRole.Customer
        };

        _userRepository.EmailExistsAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordHasher.Hash(Arg.Any<string>())
            .Returns("hashed");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Password");
    }

    [Fact]
    public async Task Handle_WithAdminRole_ShouldCreateAdminUser()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Admin User",
            Email = "admin@example.com",
            Password = "Test@123",
            Role = UserRole.Admin
        };

        _userRepository.EmailExistsAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordHasher.Hash(Arg.Any<string>())
            .Returns("hashed_password");

        _userRepository.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<User>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(UserRole.Admin);
    }
}