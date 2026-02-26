using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalForge.Api.Data;
using RentalForge.Api.Models;
using RentalForge.Api.Services;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

/// <summary>
/// Service-level integration tests for PaymentService using Testcontainers.
/// Tests use the full DI container via TestWebAppFactory.
/// </summary>
public class PaymentServiceTests : IClassFixture<TestWebAppFactory>, IAsyncLifetime
{
    private readonly TestWebAppFactory _factory;

    public PaymentServiceTests(TestWebAppFactory factory)
    {
        _factory = factory;
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
    // CreatePaymentAsync
    // =====================================================

    [Fact]
    public async Task CreatePayment_Valid_Returns_Created_With_Detail()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var request = new CreatePaymentRequest
        {
            RentalId = 9001,
            Amount = 3.99m,
            StaffId = 9001
        };

        var result = await service.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().BeGreaterThan(0);
        result.Value.RentalId.Should().Be(9001);
        result.Value.CustomerId.Should().Be(9001);
        result.Value.CustomerFirstName.Should().Be("Alice");
        result.Value.CustomerLastName.Should().Be("Anderson");
        result.Value.StaffId.Should().Be(9001);
        result.Value.StaffFirstName.Should().Be("Active");
        result.Value.StaffLastName.Should().Be("Staffer");
        result.Value.Amount.Should().Be(3.99m);
        result.Value.FilmTitle.Should().Be("Rental Test Film A");
    }

    [Fact]
    public async Task CreatePayment_Default_PaymentDate_When_Null()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var request = new CreatePaymentRequest
        {
            RentalId = 9001,
            Amount = 1.99m,
            StaffId = 9001,
            PaymentDate = null
        };

        var result = await service.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task CreatePayment_Custom_PaymentDate_Honored()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var customDate = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var request = new CreatePaymentRequest
        {
            RentalId = 9001,
            Amount = 2.50m,
            StaffId = 9001,
            PaymentDate = customDate
        };

        var result = await service.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentDate.Should().Be(customDate);
    }

    [Fact]
    public async Task CreatePayment_InvalidAmount_Returns_Invalid()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var request = new CreatePaymentRequest
        {
            RentalId = 9001,
            Amount = 0,
            StaffId = 9001
        };

        var result = await service.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "Amount");
    }

    [Fact]
    public async Task CreatePayment_NonExistentRental_Returns_Invalid()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var request = new CreatePaymentRequest
        {
            RentalId = 99999,
            Amount = 3.99m,
            StaffId = 9001
        };

        var result = await service.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e =>
            e.Identifier == "rentalId" && e.ErrorMessage.Contains("99999"));
    }

    [Fact]
    public async Task CreatePayment_InactiveStaff_Returns_Invalid()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var request = new CreatePaymentRequest
        {
            RentalId = 9001,
            Amount = 3.99m,
            StaffId = 9002 // inactive
        };

        var result = await service.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e =>
            e.Identifier == "staffId" && e.ErrorMessage.Contains("9002"));
    }

    [Fact]
    public async Task CreatePayment_AggregatedErrors_MultipleFailures()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var request = new CreatePaymentRequest
        {
            RentalId = 0,
            Amount = -1,
            StaffId = 0
        };

        var result = await service.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task CreatePayment_TwoPaymentsSameRental_BothPersist()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var request1 = new CreatePaymentRequest { RentalId = 9001, Amount = 2.00m, StaffId = 9001 };
        var request2 = new CreatePaymentRequest { RentalId = 9001, Amount = 1.99m, StaffId = 9001 };

        var result1 = await service.CreatePaymentAsync(request1);
        var result2 = await service.CreatePaymentAsync(request2);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Id.Should().NotBe(result2.Value.Id);

        // Verify both persisted
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        var count = await db.Payments.CountAsync(p => p.RentalId == 9001);
        count.Should().BeGreaterThanOrEqualTo(2);
    }

    // =====================================================
    // GetPaymentsAsync
    // =====================================================

    [Fact]
    public async Task GetPayments_Returns_PaginatedResults()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await PaymentTestHelper.SeedMultiplePaymentsAsync(db);

        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var result = await service.GetPaymentsAsync(null, null, null, null, 1, 10);

        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(4);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetPayments_FilterByCustomerId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await PaymentTestHelper.SeedMultiplePaymentsAsync(db);

        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var result = await service.GetPaymentsAsync(9001, null, null, null, 1, 10);

        result.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(p => p.CustomerId == 9001);
    }

    [Fact]
    public async Task GetPayments_FilterByStaffId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await PaymentTestHelper.SeedMultiplePaymentsAsync(db);

        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var result = await service.GetPaymentsAsync(null, 9001, null, null, 1, 10);

        result.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(p => p.StaffId == 9001);
    }

    [Fact]
    public async Task GetPayments_FilterByRentalId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await PaymentTestHelper.SeedMultiplePaymentsAsync(db);

        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var result = await service.GetPaymentsAsync(null, null, 9001, null, 1, 10);

        result.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(p => p.RentalId == 9001);
    }

    [Fact]
    public async Task GetPayments_CorrectTotalCountAndPages()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await PaymentTestHelper.SeedMultiplePaymentsAsync(db);

        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var result = await service.GetPaymentsAsync(null, null, null, null, 1, 2);

        result.TotalCount.Should().BeGreaterThanOrEqualTo(4);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPayments_EmptyResults_ReturnEmptyPage()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var result = await service.GetPaymentsAsync(99999, null, null, null, 1, 10);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
