using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalForge.Api.Data;
using RentalForge.Api.Models;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

public class PaymentEndpointTests : IClassFixture<TestWebAppFactory>, IAsyncLifetime
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _staffClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public PaymentEndpointTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _staffClient = AuthTestHelper.CreateAuthenticatedClient(
            factory, "test-staff-id", "staff@test.com", "Staff");
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        await db.Database.ExecuteSqlRawAsync(
            """
            SET session_replication_role = 'replica';
            DELETE FROM payment WHERE payment_id >= 9000;
            DELETE FROM rental WHERE rental_id >= 9000 OR inventory_id >= 9000;
            DELETE FROM inventory WHERE inventory_id >= 9000;
            DELETE FROM film WHERE film_id >= 9000;
            DELETE FROM language WHERE language_id >= 9000;
            DELETE FROM customer WHERE customer_id >= 9000;
            DELETE FROM staff WHERE staff_id >= 9000;
            DELETE FROM store WHERE store_id >= 9000;
            DELETE FROM address WHERE address_id >= 9000;
            SET session_replication_role = 'origin';
            """);

        await RentalTestHelper.SeedTestDataAsync(db);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // =====================================================
    // POST /api/payments
    // =====================================================

    [Fact]
    public async Task CreatePayment_Valid_Returns_201_With_Detail_And_Location()
    {
        var request = new CreatePaymentRequest
        {
            RentalId = 9001,
            Amount = 3.99m,
            StaffId = 9001
        };

        var response = await _staffClient.PostAsJsonAsync("/api/payments", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var detail = await response.Content.ReadFromJsonAsync<PaymentDetailResponse>(JsonOptions);
        detail.Should().NotBeNull();
        detail!.Id.Should().BeGreaterThan(0);
        detail.RentalId.Should().Be(9001);
        detail.CustomerId.Should().Be(9001);
        detail.CustomerFirstName.Should().Be("Alice");
        detail.StaffId.Should().Be(9001);
        detail.StaffFirstName.Should().Be("Active");
        detail.Amount.Should().Be(3.99m);
        detail.FilmTitle.Should().Be("Rental Test Film A");

        response.Headers.Location!.ToString().Should().Contain($"/api/payments/{detail.Id}");
    }

    [Fact]
    public async Task CreatePayment_Aggregated_Validation_Errors()
    {
        var request = new CreatePaymentRequest
        {
            RentalId = 0,
            Amount = -1,
            StaffId = 0
        };

        var response = await _staffClient.PostAsJsonAsync("/api/payments", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("RentalId");
        content.Should().Contain("Amount");
        content.Should().Contain("StaffId");
    }

    [Fact]
    public async Task CreatePayment_Unauthenticated_Returns_401()
    {
        var client = _factory.CreateClient();
        var request = new CreatePaymentRequest
        {
            RentalId = 9001,
            Amount = 3.99m,
            StaffId = 9001
        };

        var response = await client.PostAsJsonAsync("/api/payments", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePayment_CustomerRole_Returns_403()
    {
        var customerClient = AuthTestHelper.CreateAuthenticatedClient(
            _factory, "test-customer-id", "customer@test.com", "Customer");

        var request = new CreatePaymentRequest
        {
            RentalId = 9001,
            Amount = 3.99m,
            StaffId = 9001
        };

        var response = await customerClient.PostAsJsonAsync("/api/payments", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePayment_AdminRole_Returns_201()
    {
        var adminClient = AuthTestHelper.CreateAuthenticatedClient(
            _factory, "test-admin-id", "admin@test.com", "Admin");

        var request = new CreatePaymentRequest
        {
            RentalId = 9001,
            Amount = 2.99m,
            StaffId = 9001
        };

        var response = await adminClient.PostAsJsonAsync("/api/payments", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePayment_NonExistentRental_Returns_400()
    {
        var request = new CreatePaymentRequest
        {
            RentalId = 99999,
            Amount = 3.99m,
            StaffId = 9001
        };

        var response = await _staffClient.PostAsJsonAsync("/api/payments", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("99999");
    }

    [Fact]
    public async Task CreatePayment_InactiveStaff_Returns_400()
    {
        var request = new CreatePaymentRequest
        {
            RentalId = 9001,
            Amount = 3.99m,
            StaffId = 9002 // inactive
        };

        var response = await _staffClient.PostAsJsonAsync("/api/payments", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("9002");
    }

    // =====================================================
    // GET /api/payments
    // =====================================================

    [Fact]
    public async Task GetPayments_Staff_Returns_200()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await PaymentTestHelper.SeedMultiplePaymentsAsync(db);

        var response = await _staffClient.GetAsync("/api/payments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<PaymentListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPayments_Unauthenticated_Returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/payments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPayments_Pagination()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await PaymentTestHelper.SeedMultiplePaymentsAsync(db);

        var response = await _staffClient.GetAsync("/api/payments?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<PaymentListResponse>>(JsonOptions);
        result!.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(4);
    }
}
