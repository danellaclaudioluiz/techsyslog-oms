using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.ValueObjects;
using TechsysLog.Infrastructure.Services;

namespace TechsysLog.Infrastructure.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly JwtSettings _settings;

    public JwtServiceTests()
    {
        _settings = new JwtSettings
        {
            Secret = "ThisIsAVeryLongSecretKeyForTesting123!@#$%",
            Issuer = "TechsysLog.Test",
            Audience = "TechsysLog.Test.Audience",
            ExpirationInMinutes = 60
        };

        var options = Options.Create(_settings);
        _jwtService = new JwtService(options);
    }

    private static User CreateTestUser(UserRole role = UserRole.Customer)
    {
        var email = Email.Create("test@example.com").Value;
        var password = Password.FromHash("hashed_password");
        return User.Create("Test User", email, password, role).Value;
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts
    }

    [Fact]
    public void GenerateToken_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email.Value);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == user.Name);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == user.Role.ToString());
    }

    [Fact]
    public void GenerateToken_ShouldSetCorrectIssuerAndAudience()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Issuer.Should().Be(_settings.Issuer);
        jwtToken.Audiences.Should().Contain(_settings.Audience);
    }

    [Fact]
    public void GenerateToken_ShouldSetCorrectExpiration()
    {
        // Arrange
        var user = CreateTestUser();
        var expectedExpiration = DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes);

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetExpirationInSeconds_ShouldReturnCorrectValue()
    {
        // Act
        var expirationInSeconds = _jwtService.GetExpirationInSeconds();

        // Assert
        expirationInSeconds.Should().Be(_settings.ExpirationInMinutes * 60);
    }

    [Theory]
    [InlineData(UserRole.Customer)]
    [InlineData(UserRole.Operator)]
    [InlineData(UserRole.Admin)]
    public void GenerateToken_ShouldIncludeCorrectRole(UserRole role)
    {
        // Arrange
        var user = CreateTestUser(role);

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role.ToString());
    }

    [Fact]
    public void GenerateToken_ShouldGenerateUniqueJti()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token1 = _jwtService.GenerateToken(user);
        var token2 = _jwtService.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jti1 = handler.ReadJwtToken(token1).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = handler.ReadJwtToken(token2).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        // Assert
        jti1.Should().NotBe(jti2);
    }
}