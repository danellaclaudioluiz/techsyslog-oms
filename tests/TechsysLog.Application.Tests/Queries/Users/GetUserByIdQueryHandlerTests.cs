using AutoMapper;
using FluentAssertions;
using NSubstitute;
using TechsysLog.Application.Mappings;
using TechsysLog.Application.Queries.Users;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Tests.Queries.Users;

public class GetUserByIdQueryHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new GetUserByIdQueryHandler(_userRepository, _mapper);
    }

    private static User CreateTestUser()
    {
        var email = Email.Create("test@example.com").Value;
        var password = Password.FromHash("hashed_password");
        return User.Create("Test User", email, password, UserRole.Customer).Value;
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldReturnUserDto()
    {
        // Arrange
        var user = CreateTestUser();
        var query = new GetUserByIdQuery { UserId = user.Id };

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Name.Should().Be("Test User");
        result.Email.Should().Be("test@example.com");
        result.Role.Should().Be(UserRole.Customer);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var query = new GetUserByIdQuery { UserId = Guid.NewGuid() };

        _userRepository.GetByIdAsync(query.UserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}