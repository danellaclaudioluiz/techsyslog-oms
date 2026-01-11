using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TechsysLog.API.Controllers;
using TechsysLog.API.Models;
using TechsysLog.Application.DTOs;
using TechsysLog.Integration.Tests.Fixtures;

namespace TechsysLog.Integration.Tests.Api;

public class AuthApiTests : IClassFixture<MongoDbFixture>, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly MongoDbFixture _mongoFixture;

    public AuthApiTests(MongoDbFixture mongoFixture, WebApplicationFactory<Program> factory)
    {
        _mongoFixture = mongoFixture;

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override MongoDB connection string
                services.Configure<TechsysLog.Infrastructure.Persistence.MongoDbSettings>(options =>
                {
                    options.ConnectionString = _mongoFixture.Settings.ConnectionString;
                    options.DatabaseName = "techsyslog_test_" + Guid.NewGuid().ToString("N")[..8];
                });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn201()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "Test@123456",
            Role = "Customer"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Test User");
        result.Data.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "invalid-email",
            Password = "Test@123456",
            Role = "Customer"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturn400()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "weak",
            Role = "Customer"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200WithToken()
    {
        // Arrange - First register a user
        var email = $"login_test_{Guid.NewGuid():N}@example.com";
        var password = "Test@123456";

        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Name = "Login Test User",
            Email = email,
            Password = password,
            Role = "Customer"
        });

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401()
    {
        // Arrange - First register a user
        var email = $"invalid_pass_{Guid.NewGuid():N}@example.com";

        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Name = "Test User",
            Email = email,
            Password = "Test@123456",
            Role = "Customer"
        });

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "WrongPassword123!"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturn401()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Test@123456"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ShouldReturn200()
    {
        // Arrange - Register and login
        var email = $"getme_test_{Guid.NewGuid():N}@example.com";
        var password = "Test@123456";

        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Name = "GetMe Test User",
            Email = email,
            Password = password,
            Role = "Customer"
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        var token = loginResult!.Data!.Token;

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        result!.Data!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetMe_WithoutToken_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}