using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalForge.Api.Data;
using RentalForge.Api.Models.Auth;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

public class AuthEndpointTests : IClassFixture<TestWebAppFactory>, IAsyncLifetime
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthEndpointTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Clean up auth data between tests
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // =====================================================
    // US1: POST /api/auth/register — Account Registration
    // =====================================================

    [Fact]
    public async Task Register_ValidRequest_Returns201WithTokens()
    {
        var request = new RegisterRequest
        {
            Email = $"register-test-{Guid.NewGuid():N}@example.com",
            Password = "SecureP@ss1"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(request.Email);
        result.User.Role.Should().Be("Customer");
        result.User.CustomerId.Should().BeNull();
        result.User.StaffId.Should().BeNull();
        result.User.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest { Email = email, Password = "SecureP@ss1" };

        // First registration succeeds
        var first = await _client.PostAsJsonAsync("/api/auth/register", request);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        // Second registration fails
        var second = await _client.PostAsJsonAsync("/api/auth/register", request);
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await second.Content.ReadAsStringAsync();
        var problem = JsonDocument.Parse(content).RootElement;
        problem.GetProperty("errors").TryGetProperty("Email", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Register_InvalidPassword_Returns400WithAllErrors()
    {
        var request = new RegisterRequest
        {
            Email = $"weak-{Guid.NewGuid():N}@example.com",
            Password = "weak" // too short, missing upper, digit, special
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var problem = JsonDocument.Parse(content).RootElement;
        var errors = problem.GetProperty("errors");
        errors.TryGetProperty("Password", out var passwordErrors).Should().BeTrue();
        passwordErrors.GetArrayLength().Should().BeGreaterThan(1,
            "all password errors should be aggregated");
    }

    [Fact]
    public async Task Register_ElevatedRoleWithoutAdmin_Returns403()
    {
        var request = new RegisterRequest
        {
            Email = $"elevated-{Guid.NewGuid():N}@example.com",
            Password = "SecureP@ss1",
            Role = "Admin"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Register_ElevatedRoleWithAdminToken_Returns201()
    {
        // Create an admin user first
        var adminUser = await AuthTestHelper.CreateTestUserAsync(
            _factory.Services,
            $"admin-{Guid.NewGuid():N}@example.com",
            "Admin");

        var adminClient = AuthTestHelper.CreateAuthenticatedClient(
            _factory, adminUser.Id, adminUser.Email!, "Admin");

        var request = new RegisterRequest
        {
            Email = $"staff-{Guid.NewGuid():N}@example.com",
            Password = "SecureP@ss1",
            Role = "Staff"
        };

        var response = await adminClient.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.User.Role.Should().Be("Staff");
    }

    [Fact]
    public async Task Register_EmptyFields_Returns400()
    {
        var request = new RegisterRequest { Email = "", Password = "" };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_TokenIsValidJwt()
    {
        var email = $"jwt-test-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest { Email = email, Password = "SecureP@ss1" };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", request);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        result.Should().NotBeNull();

        // Use the returned token to access /me
        var authedClient = _factory.CreateClient();
        AuthTestHelper.SetAuthToken(authedClient, result!.Token);

        var meResponse = await authedClient.GetAsync("/api/auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await meResponse.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        user.Should().NotBeNull();
        user!.Email.Should().Be(email);
    }

    // =====================================================
    // US2: POST /api/auth/login — Login
    // =====================================================

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        await AuthTestHelper.CreateTestUserAsync(_factory.Services, email, "Staff");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = AuthTestHelper.DefaultPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be(email);
        result.User.Role.Should().Be("Staff");
    }

    [Fact]
    public async Task Login_UserWithStaffId_ReturnsStaffIdInResponse()
    {
        // Seed rental test data to create staff record with ID 9001
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
            if (!await db.Staff.AnyAsync(s => s.StaffId == 9001))
                await RentalTestHelper.SeedTestDataAsync(db);
        }

        var email = $"staff-link-{Guid.NewGuid():N}@example.com";
        await AuthTestHelper.CreateTestUserAsync(_factory.Services, email, "Staff", staffId: 9001);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = AuthTestHelper.DefaultPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.User.StaffId.Should().Be(9001);
    }

    [Fact]
    public async Task Login_UserWithoutStaffId_ReturnsNullStaffId()
    {
        var email = $"no-staff-{Guid.NewGuid():N}@example.com";
        await AuthTestHelper.CreateTestUserAsync(_factory.Services, email, "Customer");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = AuthTestHelper.DefaultPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.User.StaffId.Should().BeNull();
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401GenericMessage()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "WrongP@ss1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        var problem = JsonDocument.Parse(content).RootElement;
        problem.GetProperty("title").GetString().Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"wrongpw-{Guid.NewGuid():N}@example.com";
        await AuthTestHelper.CreateTestUserAsync(_factory.Services, email, "Customer");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "WrongP@ssword1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_EmptyFields_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "",
            Password = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // =====================================================
    // US2: POST /api/auth/logout — Logout
    // =====================================================

    [Fact]
    public async Task Logout_ValidToken_Returns204()
    {
        // Register and get tokens
        var email = $"logout-{Guid.NewGuid():N}@example.com";
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "SecureP@ss1"
        });
        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        var authedClient = _factory.CreateClient();
        AuthTestHelper.SetAuthToken(authedClient, auth!.Token);

        var response = await authedClient.PostAsJsonAsync("/api/auth/logout", new LogoutRequest
        {
            RefreshToken = auth.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_InvalidRefreshToken_Returns204_Idempotent()
    {
        var email = $"logout-idem-{Guid.NewGuid():N}@example.com";
        var user = await AuthTestHelper.CreateTestUserAsync(_factory.Services, email, "Customer");
        var authedClient = AuthTestHelper.CreateAuthenticatedClient(_factory, user.Id, email, "Customer");

        var response = await authedClient.PostAsJsonAsync("/api/auth/logout", new LogoutRequest
        {
            RefreshToken = "totally-invalid-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_Unauthenticated_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/logout", new LogoutRequest
        {
            RefreshToken = "some-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // =====================================================
    // US2: GET /api/auth/me — Get Current User
    // =====================================================

    [Fact]
    public async Task Me_AuthenticatedUser_Returns200WithUserInfo()
    {
        var email = $"me-{Guid.NewGuid():N}@example.com";
        var user = await AuthTestHelper.CreateTestUserAsync(_factory.Services, email, "Admin");
        var authedClient = AuthTestHelper.CreateAuthenticatedClient(_factory, user.Id, email, "Admin");

        var response = await authedClient.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(email);
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Me_UserWithStaffId_ReturnsStaffId()
    {
        // Seed rental test data to create staff record with ID 9001
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
            if (!await db.Staff.AnyAsync(s => s.StaffId == 9001))
                await RentalTestHelper.SeedTestDataAsync(db);
        }

        var email = $"me-staff-{Guid.NewGuid():N}@example.com";
        var user = await AuthTestHelper.CreateTestUserAsync(
            _factory.Services, email, "Staff", staffId: 9001);
        var authedClient = AuthTestHelper.CreateAuthenticatedClient(
            _factory, user.Id, email, "Staff");

        var response = await authedClient.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.StaffId.Should().Be(9001);
    }

    [Fact]
    public async Task Me_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // =====================================================
    // US4: POST /api/auth/refresh — Token Refresh
    // =====================================================

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokens()
    {
        // Register and get initial tokens
        var email = $"refresh-{Guid.NewGuid():N}@example.com";
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "SecureP@ss1"
        });
        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        // Refresh
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = auth!.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RefreshResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(auth.RefreshToken, "new refresh token should be different");
    }

    [Fact]
    public async Task Refresh_OldTokenConsumed_ReturnsUnauthorized()
    {
        // Register and get tokens
        var email = $"refresh-reuse-{Guid.NewGuid():N}@example.com";
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "SecureP@ss1"
        });
        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        // First refresh succeeds
        var first = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = auth!.RefreshToken
        });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        // Second refresh with same token fails (consumed)
        var second = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = auth.RefreshToken
        });
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ReuseTriggersFamily_Invalidation()
    {
        // Register and get tokens
        var email = $"refresh-family-{Guid.NewGuid():N}@example.com";
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "SecureP@ss1"
        });
        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        // First refresh succeeds — returns new token
        var firstRefresh = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = auth!.RefreshToken
        });
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await firstRefresh.Content.ReadFromJsonAsync<RefreshResponse>(JsonOptions);

        // Reuse old token — triggers family invalidation
        var reuse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = auth.RefreshToken
        });
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // New token is also invalidated (family revoked)
        var afterReuse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = newTokens!.RefreshToken
        });
        afterReuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = "completely-invalid-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_NewTokenWorksForSubsequentRefresh()
    {
        // Register
        var email = $"refresh-chain-{Guid.NewGuid():N}@example.com";
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "SecureP@ss1"
        });
        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        // First refresh
        var first = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = auth!.RefreshToken
        });
        var firstResult = await first.Content.ReadFromJsonAsync<RefreshResponse>(JsonOptions);

        // Second refresh with new token
        var second = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = firstResult!.RefreshToken
        });
        second.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_EmptyToken_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
