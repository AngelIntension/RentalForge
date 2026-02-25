using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalForge.Api.Data;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

public class AuthorizationTests : IClassFixture<TestWebAppFactory>, IAsyncLifetime
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _anonymousClient;
    private static readonly string TestCustomerEmail = $"authz-customer-{Guid.NewGuid():N}@example.com";
    private static readonly string TestStaffEmail = $"authz-staff-{Guid.NewGuid():N}@example.com";
    private static readonly string TestAdminEmail = $"authz-admin-{Guid.NewGuid():N}@example.com";
    private static readonly string TestUnlinkedEmail = $"authz-unlinked-{Guid.NewGuid():N}@example.com";

    private HttpClient _customerClient = null!;
    private HttpClient _staffClient = null!;
    private HttpClient _adminClient = null!;
    private HttpClient _unlinkedCustomerClient = null!;
    private int _linkedCustomerId;

    public AuthorizationTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _anonymousClient = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Ensure test data exists
        var hasTestStore = await db.Stores.AnyAsync(s => s.StoreId == 9001);
        if (!hasTestStore)
            await CustomerTestHelper.SeedTestDataAsync(db);

        // Customer 9001 (Alice) is active
        _linkedCustomerId = 9001;

        // Create test users
        var customerUser = await AuthTestHelper.CreateTestUserAsync(
            _factory.Services, TestCustomerEmail, "Customer", _linkedCustomerId);
        var staffUser = await AuthTestHelper.CreateTestUserAsync(
            _factory.Services, TestStaffEmail, "Staff");
        var adminUser = await AuthTestHelper.CreateTestUserAsync(
            _factory.Services, TestAdminEmail, "Admin");
        var unlinkedUser = await AuthTestHelper.CreateTestUserAsync(
            _factory.Services, TestUnlinkedEmail, "Customer"); // no customerId

        _customerClient = AuthTestHelper.CreateAuthenticatedClient(
            _factory, customerUser.Id, TestCustomerEmail, "Customer");
        _staffClient = AuthTestHelper.CreateAuthenticatedClient(
            _factory, staffUser.Id, TestStaffEmail, "Staff");
        _adminClient = AuthTestHelper.CreateAuthenticatedClient(
            _factory, adminUser.Id, TestAdminEmail, "Admin");
        _unlinkedCustomerClient = AuthTestHelper.CreateAuthenticatedClient(
            _factory, unlinkedUser.Id, TestUnlinkedEmail, "Customer");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // =====================================================
    // Health — AllowAnonymous
    // =====================================================

    [Fact]
    public async Task Health_Anonymous_Returns200()
    {
        var response = await _anonymousClient.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // =====================================================
    // Films — GET: any authenticated; POST/PUT/DELETE: Staff/Admin
    // =====================================================

    [Fact]
    public async Task Films_Get_Anonymous_Returns401()
    {
        var response = await _anonymousClient.GetAsync("/api/films");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Films_Get_Customer_Returns200()
    {
        var response = await _customerClient.GetAsync("/api/films");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Films_Get_Staff_Returns200()
    {
        var response = await _staffClient.GetAsync("/api/films");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Films_Post_Customer_Returns403()
    {
        var response = await _customerClient.PostAsJsonAsync("/api/films", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Films_Post_Staff_DoesNotReturn401Or403()
    {
        // Staff can POST — may get 400 for invalid body, but not 401/403
        var response = await _staffClient.PostAsJsonAsync("/api/films", new { Title = "Test" });
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    // =====================================================
    // Customers — List: Staff/Admin only
    // =====================================================

    [Fact]
    public async Task Customers_List_Anonymous_Returns401()
    {
        var response = await _anonymousClient.GetAsync("/api/customers");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Customers_List_Customer_Returns403()
    {
        var response = await _customerClient.GetAsync("/api/customers");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Customers_List_Staff_Returns200()
    {
        var response = await _staffClient.GetAsync("/api/customers");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // =====================================================
    // Customers — GET by ID: Customer can see own record
    // =====================================================

    [Fact]
    public async Task Customers_GetOwn_Customer_Returns200()
    {
        var response = await _customerClient.GetAsync($"/api/customers/{_linkedCustomerId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Customers_GetOther_Customer_Returns403()
    {
        // Customer 9002 is not the linked customer
        var response = await _customerClient.GetAsync("/api/customers/9002");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Customers_GetById_UnlinkedCustomer_Returns403()
    {
        // Unlinked customer (no customerId) cannot view any customer record
        var response = await _unlinkedCustomerClient.GetAsync($"/api/customers/{_linkedCustomerId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Customers_GetById_Staff_Returns200()
    {
        var response = await _staffClient.GetAsync($"/api/customers/{_linkedCustomerId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // =====================================================
    // Customers — POST: Staff/Admin only
    // =====================================================

    [Fact]
    public async Task Customers_Post_Customer_Returns403()
    {
        var response = await _customerClient.PostAsJsonAsync("/api/customers", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // =====================================================
    // Rentals — GET: Customer sees own only; Staff/Admin see all
    // =====================================================

    [Fact]
    public async Task Rentals_List_Anonymous_Returns401()
    {
        var response = await _anonymousClient.GetAsync("/api/rentals");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Rentals_List_Staff_Returns200()
    {
        var response = await _staffClient.GetAsync("/api/rentals");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Rentals_Post_Customer_Returns403()
    {
        var response = await _customerClient.PostAsJsonAsync("/api/rentals", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // =====================================================
    // Auth endpoints — AllowAnonymous (login, register, refresh)
    // =====================================================

    [Fact]
    public async Task AuthMe_Anonymous_Returns401()
    {
        var response = await _anonymousClient.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthMe_Customer_Returns200()
    {
        var response = await _customerClient.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
