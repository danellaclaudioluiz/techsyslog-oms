using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TechsysLog.API.Controllers;
using TechsysLog.API.Models;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Integration.Tests.Fixtures;

namespace TechsysLog.Integration.Tests.Api;

public class OrdersApiTests : IClassFixture<MongoDbFixture>, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersApiTests(MongoDbFixture mongoFixture, WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<TechsysLog.Infrastructure.Persistence.MongoDbSettings>(options =>
                {
                    options.ConnectionString = mongoFixture.Settings.ConnectionString;
                    options.DatabaseName = "techsyslog_test_" + Guid.NewGuid().ToString("N")[..8];
                });
            });
        }).CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var email = $"admin_{Guid.NewGuid():N}@example.com";
        var password = "Admin@123456";

        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Name = "Admin User",
            Email = email,
            Password = password,
            Role = "Admin"
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        var result = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        return result!.Data!.Token;
    }

    private void SetAuthHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ShouldReturn201()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var request = new CreateOrderRequest
        {
            Description = "Test Order",
            Value = 199.99m,
            Cep = "01310100",
            Number = "100",
            Complement = "Sala 1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        result!.Success.Should().BeTrue();
        result.Data!.Description.Should().Be("Test Order");
        result.Data.Value.Should().Be(199.99m);
        result.Data.OrderNumber.Should().MatchRegex(@"^ORD-\d{8}-\d{5}$");
        result.Data.DeliveryAddress.Should().NotBeNull();
        result.Data.DeliveryAddress!.Street.Should().Be("Avenida Paulista");
    }

    [Fact]
    public async Task CreateOrder_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        var request = new CreateOrderRequest
        {
            Description = "Test Order",
            Value = 199.99m,
            Cep = "01310100",
            Number = "100"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidCep_ShouldReturn400()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var request = new CreateOrderRequest
        {
            Description = "Test Order",
            Value = 199.99m,
            Cep = "invalid",
            Number = "100"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrders_WithAuth_ShouldReturn200()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        // Create an order first
        await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest
        {
            Description = "Test Order",
            Value = 100m,
            Cep = "01310100",
            Number = "100"
        });

        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<OrderDto>>>();
        result!.Success.Should().BeTrue();
        result.Data!.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetOrderById_WithExistingOrder_ShouldReturn200()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var createResponse = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest
        {
            Description = "Test Order",
            Value = 100m,
            Cep = "01310100",
            Number = "100"
        });

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        var orderId = createResult!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        result!.Data!.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task GetOrderById_WithNonExistentOrder_ShouldReturn404()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidTransition_ShouldReturn200()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var createResponse = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest
        {
            Description = "Test Order",
            Value = 100m,
            Cep = "01310100",
            Number = "100"
        });

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        var orderId = createResult!.Data!.Id;

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/orders/{orderId}/status", new UpdateOrderStatusRequest
        {
            Status = "Confirmed"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        result!.Data!.Status.Should().Be(Domain.Enums.OrderStatus.Confirmed);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithInvalidTransition_ShouldReturn400()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var createResponse = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest
        {
            Description = "Test Order",
            Value = 100m,
            Cep = "01310100",
            Number = "100"
        });

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        var orderId = createResult!.Data!.Id;

        // Act - Try to go from Pending directly to Delivered
        var response = await _client.PatchAsJsonAsync($"/api/orders/{orderId}/status", new UpdateOrderStatusRequest
        {
            Status = "Delivered"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}