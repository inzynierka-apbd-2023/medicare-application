using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;

namespace AppointmentService.Tests.Integration;

public class AnalyticsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AnalyticsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppointmentDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<AppointmentDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboardAnalytics_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/appointment/analytics/dashboard");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentMetrics_WithInvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        SetupAuthentication();
        var startDate = DateTime.UtcNow.AddDays(1); // Future date
        var endDate = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync($"/api/appointment/analytics/metrics?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSpecializationStats_WithValidAuth_ReturnsData()
    {
        // Arrange
        SetupAuthentication("Admin");

        // Act
        var response = await _client.GetAsync("/api/appointment/analytics/specialization-stats");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var stats = JsonSerializer.Deserialize<IEnumerable<SpecializationStatsDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(stats);
    }

    private void SetupAuthentication(string role = "Admin")
    {
        // In a real scenario, you would set up proper JWT tokens
        // For this test, we'll assume the authentication middleware is bypassed in test environment
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
    }
}
