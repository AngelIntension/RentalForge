using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RentalForge.Api.Models.Auth;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

public class RateLimitTests : IClassFixture<RateLimitTestFactory>
{
    private readonly HttpClient _client;

    public RateLimitTests(RateLimitTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ExceedsLimit_Returns429WithRetryAfter()
    {
        // auth-register policy: 3 per minute
        for (var i = 0; i < 3; i++)
        {
            var request = new RegisterRequest
            {
                Email = $"ratelimit-reg-{i}-{Guid.NewGuid():N}@example.com",
                Password = "SecureP@ss1"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests,
                $"request {i + 1} of 3 should be within rate limit");
        }

        // 4th request exceeds the limit
        var limited = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest
            {
                Email = $"ratelimit-excess-{Guid.NewGuid():N}@example.com",
                Password = "SecureP@ss1"
            });

        limited.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        limited.Headers.RetryAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_ExceedsLimit_Returns429WithRetryAfter()
    {
        // auth-login policy: 5 per minute
        for (var i = 0; i < 5; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest
                {
                    Email = $"ratelimit-login-{i}@example.com",
                    Password = "AnyP@ss1"
                });
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests,
                $"request {i + 1} of 5 should be within rate limit");
        }

        // 6th request exceeds the limit
        var limited = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "excess@example.com", Password = "AnyP@ss1" });

        limited.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        limited.Headers.RetryAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task Refresh_ExceedsLimit_Returns429WithRetryAfter()
    {
        // auth-refresh policy: 10 per minute
        for (var i = 0; i < 10; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/refresh",
                new RefreshRequest { RefreshToken = $"fake-token-{i}" });
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests,
                $"request {i + 1} of 10 should be within rate limit");
        }

        // 11th request exceeds the limit
        var limited = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest { RefreshToken = "excess-token" });

        limited.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        limited.Headers.RetryAfter.Should().NotBeNull();
    }
}
