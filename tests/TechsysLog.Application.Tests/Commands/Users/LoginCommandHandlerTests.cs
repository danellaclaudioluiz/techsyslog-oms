using AutoMapper;
using FluentAssertions;
using NSubstitute;
using TechsysLog.Application.Commands.Users;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Mappings;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Tests.Commands.Users;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtService = Substitute.For<IJwtService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new LoginCommandHandler(
            _userRepository,
            _passwordHasher,
            _jwtService,
            _mapper);
    }

    private static User CreateTestUser()
    {
        var email = Email.Create("test@example.com").Value;
        var password = Password.FromHash("hashed_password");
        return User.Create("Test User", email, password, UserRole.Customer).Value;
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Test@123"
        };

        var user = CreateTestUser();

        _userRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify(command.Password, user.Password.Hash)
            .Returns(true);

        _jwtService.GenerateToken(user)
            .Returns("jwt_token_here");

        _jwtService.GetExpirationInSeconds()
            .Returns(3600);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("jwt_token_here");
        result.Value.TokenType.Should().Be("Bearer");
        result.Value.ExpiresIn.Should().Be(3600);
        result.Value.User.Should().NotBeNull();
        result.Value.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "invalid-email",
            Password = "Test@123"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "Test@123"
        };

        _userRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword@123"
        };

        var user = CreateTestUser();

        _userRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify(command.Password, user.Password.Hash)
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid credentials.");

        _jwtService.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ShouldNotRevealWhetherEmailExists()
    {
        // Arrange
        var commandWithNonExistentEmail = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "Test@123"
        };

        var commandWithWrongPassword = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword@123"
        };

        var user = CreateTestUser();

        _userRepository.GetByEmailAsync(
            Arg.Is<Email>(e => e.Value == "nonexistent@example.com"),
            Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _userRepository.GetByEmailAsync(
            Arg.Is<Email>(e => e.Value == "test@example.com"),
            Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        // Act
        var result1 = await _handler.Handle(commandWithNonExistentEmail, CancellationToken.None);
        var result2 = await _handler.Handle(commandWithWrongPassword, CancellationToken.None);

        // Assert - Both should return same error message for security
        result1.Error.Should().Be(result2.Error);
    }
}