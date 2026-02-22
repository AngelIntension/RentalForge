using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

public class HealthEndpointTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk_WhenDatabaseIsReachable()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/health");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content).RootElement;

        json.GetProperty("status").GetString().Should().Be("healthy");
        json.GetProperty("databaseVersion").GetString().Should().Contain("PostgreSQL");
        json.GetProperty("serverTime").GetString().Should().NotBeNullOrEmpty();

        // Verify serverTime is a valid ISO 8601 timestamp
        DateTimeOffset.Parse(json.GetProperty("serverTime").GetString()!).Should().BeCloseTo(
            DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));

        // SC-001: Response time under 2 seconds
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task HealthEndpoint_Returns503_WhenDatabaseIsUnreachable()
    {
        // Arrange — factory with invalid connection string
        await using var unreachableFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Dvdrental",
                    "Host=localhost;Port=19999;Database=dvdrental;Username=postgres;Password=test;Timeout=2");
            });

        using var client = unreachableFactory.CreateClient();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync("/health");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content).RootElement;

        json.GetProperty("status").GetString().Should().Be("unhealthy");
        json.GetProperty("error").GetString().Should().NotBeNullOrEmpty();

        // SC-002: Response time under 5 seconds
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SwaggerMetadata_ContainsHealthEndpoint_WithCorrectAttributes()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var swagger = JsonDocument.Parse(content).RootElement;

        var healthPath = swagger.GetProperty("paths").GetProperty("/health").GetProperty("get");

        healthPath.GetProperty("operationId").GetString().Should().Be("HealthCheck");
        healthPath.GetProperty("summary").GetString().Should().Be("Database health check");
        healthPath.GetProperty("responses").GetProperty("200")
            .GetProperty("description").GetString()
            .Should().Be("Database is healthy and reachable");
        healthPath.GetProperty("responses").GetProperty("503")
            .GetProperty("description").GetString()
            .Should().Be("Database is unhealthy or unreachable");
    }

    [Fact]
    public void App_FailsFast_WhenConnectionStringMissing()
    {
        // Arrange — factory with empty connection string
        using var emptyFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Dvdrental", "");
            });

        // Act & Assert — creating the client starts the host, which should throw
        Action act = () => emptyFactory.CreateClient();

        act.Should().Throw<Exception>()
            .Which.ToString().Should().ContainAny("connection string", "Dvdrental", "ConnectionStrings");
    }
}
