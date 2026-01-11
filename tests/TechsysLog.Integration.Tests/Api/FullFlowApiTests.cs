using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TechsysLog.API.Controllers;
using TechsysLog.API.Models;
using TechsysLog.Application.DTOs;
using TechsysLog.Domain.Enums;
using TechsysLog.Integration.Tests.Fixtures;

namespace TechsysLog.Integration.Tests.Api;

public class FullFlowApiTests : IClassFixture<MongoDbFixture>, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FullFlowApiTests(MongoDbFixture mongoFixture, WebApplicationFactory<Program> factory)
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

    [Fact]
    public async Task FullOrderLifecycle_ShouldCreateNotificationsAtEachStep()
    {
        // 1. Register admin user
        var email = $"fullflow_{Guid.NewGuid():N}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Name = "Full Flow Admin",
            Email = email,
            Password = "Admin@123456",
            Role = "Admin"
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2. Login
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "Admin@123456"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        var token = loginResult!.Data!.Token;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 3. Create order
        var createOrderResponse = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest
        {
            Description = "Full Flow Test Order",
            Value = 500m,
            Cep = "01310100",
            Number = "1000",
            Complement = "Andar 10"
        });
        createOrderResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var orderResult = await createOrderResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        var orderId = orderResult!.Data!.Id;

        // 4. Update status to Confirmed
        var confirmResponse = await _client.PatchAsJsonAsync($"/api/orders/{orderId}/status", new UpdateOrderStatusRequest
        {
            Status = "Confirmed"
        });
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Update status to InTransit
        var transitResponse = await _client.PatchAsJsonAsync($"/api/orders/{orderId}/status", new UpdateOrderStatusRequest
        {
            Status = "InTransit"
        });
        transitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Register delivery
        var deliveryResponse = await _client.PostAsJsonAsync("/api/deliveries", new RegisterDeliveryRequest
        {
            OrderId = orderId
        });
        deliveryResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 7. Check final order status
        var finalOrderResponse = await _client.GetAsync($"/api/orders/{orderId}");
        var finalOrder = await finalOrderResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        finalOrder!.Data!.Status.Should().Be(OrderStatus.Delivered);

        // 8. Check notifications were created
        var notificationsResponse = await _client.GetAsync("/api/notifications");
        var notifications = await notificationsResponse.Content.ReadFromJsonAsync<ApiResponse<List<NotificationDto>>>();

        notifications!.Data.Should().HaveCount(4);
        notifications.Data.Should().Contain(n => n.Type == NotificationType.OrderCreated);
        notifications.Data.Should().Contain(n => n.Type == NotificationType.OrderStatusChanged && n.Message.Contains("Confirmed"));
        notifications.Data.Should().Contain(n => n.Type == NotificationType.OrderStatusChanged && n.Message.Contains("InTransit"));
        notifications.Data.Should().Contain(n => n.Type == NotificationType.OrderDelivered);
    }

    [Fact]
    public async Task MarkNotificationAsRead_ShouldUpdateReadStatus()
    {
        // Setup
        var email = $"notify_{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Name = "Notification Test",
            Email = email,
            Password = "Test@123456",
            Role = "Admin"
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "Test@123456"
        });
        var token = (await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>())!.Data!.Token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create order to generate notification
        await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest
        {
            Description = "Notification Test",
            Value = 100m,
            Cep = "01310100",
            Number = "100"
        });

        // Get notifications
        var notificationsResponse = await _client.GetAsync("/api/notifications");
        var notifications = await notificationsResponse.Content.ReadFromJsonAsync<ApiResponse<List<NotificationDto>>>();
        var notificationId = notifications!.Data!.First().Id;

        // Mark as read
        var markReadResponse = await _client.PatchAsync($"/api/notifications/{notificationId}/read", null);
        markReadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify unread count
        var unreadResponse = await _client.GetAsync("/api/notifications/unread-count");
        var unreadResult = await unreadResponse.Content.ReadFromJsonAsync<ApiResponse<UnreadCountResponse>>();
        unreadResult!.Data!.Count.Should().Be(0);
    }
}