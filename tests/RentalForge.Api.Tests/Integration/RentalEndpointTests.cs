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

public class RentalEndpointTests : IClassFixture<TestWebAppFactory>, IAsyncLifetime
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public RentalEndpointTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = AuthTestHelper.CreateAuthenticatedClient(
            factory, "test-staff-id", "staff@test.com", "Staff");
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Clean up test data via raw SQL to handle circular FKs
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
    // US1: POST /api/rentals — Rent a Film
    // =====================================================

    [Fact]
    public async Task CreateRental_Valid_Returns_201_With_Detail()
    {
        // Film A (9001) at store 1 — inventory 9002 is returned, 9003 is available
        // Inventory 9001 is actively rented. Lowest available = 9002 (returned rental).
        // Actually: 9001 is rented (active), 9002 has returned rental (available), 9003 is never rented (available)
        // Deterministic: lowest available = 9002
        var request = new CreateRentalRequest
        {
            FilmId = 9001,
            StoreId = 9001,
            CustomerId = 9001,
            StaffId = 9001
        };

        var response = await _client.PostAsJsonAsync("/api/rentals", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var detail = await response.Content.ReadFromJsonAsync<RentalDetailResponse>(JsonOptions);
        detail.Should().NotBeNull();
        detail!.Id.Should().BeGreaterThan(0);
        detail.RentalDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
        detail.ReturnDate.Should().BeNull();
        detail.InventoryId.Should().Be(9002); // lowest available inventory
        detail.FilmId.Should().Be(9001);
        detail.FilmTitle.Should().Be("Rental Test Film A");
        detail.StoreId.Should().Be(9001);
        detail.CustomerId.Should().Be(9001);
        detail.CustomerFirstName.Should().Be("Alice");
        detail.CustomerLastName.Should().Be("Anderson");
        detail.StaffId.Should().Be(9001);
        detail.StaffFirstName.Should().Be("Active");
        detail.StaffLastName.Should().Be("Staffer");

        // Location header should point to the created rental
        response.Headers.Location!.ToString().Should().Contain($"/api/rentals/{detail.Id}");
    }

    [Fact]
    public async Task CreateRental_Film_Not_Stocked_At_Store_Returns_400()
    {
        // Film B (9002) is only stocked at store 1, not store 2
        var request = new CreateRentalRequest
        {
            FilmId = 9002,
            StoreId = 9002,
            CustomerId = 9001,
            StaffId = 9001
        };

        var response = await _client.PostAsJsonAsync("/api/rentals", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("not stocked at store 9002");
    }

    [Fact]
    public async Task CreateRental_All_Copies_Rented_Returns_400()
    {
        // Seed so all copies of Film B at store 1 are rented
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await RentalTestHelper.SeedAllCopiesRentedAsync(db);

        var request = new CreateRentalRequest
        {
            FilmId = 9002,
            StoreId = 9001,
            CustomerId = 9001,
            StaffId = 9001
        };

        var response = await _client.PostAsJsonAsync("/api/rentals", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("currently rented out");
    }

    [Fact]
    public async Task CreateRental_NonExistent_FilmId_Returns_400()
    {
        var request = new CreateRentalRequest
        {
            FilmId = 99999,
            StoreId = 9001,
            CustomerId = 9001,
            StaffId = 9001
        };

        var response = await _client.PostAsJsonAsync("/api/rentals", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Film with ID 99999 does not exist");
    }

    [Fact]
    public async Task CreateRental_NonExistent_StoreId_Returns_400()
    {
        var request = new CreateRentalRequest
        {
            FilmId = 9001,
            StoreId = 99999,
            CustomerId = 9001,
            StaffId = 9001
        };

        var response = await _client.PostAsJsonAsync("/api/rentals", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Store with ID 99999 does not exist");
    }

    [Fact]
    public async Task CreateRental_Inactive_Customer_Returns_400()
    {
        var request = new CreateRentalRequest
        {
            FilmId = 9001,
            StoreId = 9001,
            CustomerId = 9003, // inactive customer
            StaffId = 9001
        };

        var response = await _client.PostAsJsonAsync("/api/rentals", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Customer with ID 9003 does not exist or is inactive");
    }

    [Fact]
    public async Task CreateRental_Inactive_Staff_Returns_400()
    {
        var request = new CreateRentalRequest
        {
            FilmId = 9001,
            StoreId = 9001,
            CustomerId = 9001,
            StaffId = 9002 // inactive staff
        };

        var response = await _client.PostAsJsonAsync("/api/rentals", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Staff member with ID 9002 does not exist or is inactive");
    }

    [Fact]
    public async Task CreateRental_Multiple_Validation_Errors_Aggregated()
    {
        var request = new CreateRentalRequest
        {
            FilmId = 99999,
            StoreId = 99999,
            CustomerId = 99999,
            StaffId = 99999
        };

        var response = await _client.PostAsJsonAsync("/api/rentals", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Film with ID 99999 does not exist");
        content.Should().Contain("Store with ID 99999 does not exist");
        content.Should().Contain("Customer with ID 99999 does not exist or is inactive");
        content.Should().Contain("Staff member with ID 99999 does not exist or is inactive");
    }

    [Fact]
    public async Task CreateRental_FilmId_Zero_Returns_400_Validator()
    {
        var request = new CreateRentalRequest
        {
            FilmId = 0,
            StoreId = 9001,
            CustomerId = 9001,
            StaffId = 9001
        };

        var response = await _client.PostAsJsonAsync("/api/rentals", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRental_Deterministic_Inventory_Selection()
    {
        // Film A at store 1: inventories 9001 (rented), 9002 (available), 9003 (available)
        // First create should get 9002 (lowest available)
        var request = new CreateRentalRequest
        {
            FilmId = 9001,
            StoreId = 9001,
            CustomerId = 9001,
            StaffId = 9001
        };

        var response1 = await _client.PostAsJsonAsync("/api/rentals", request);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        var detail1 = await response1.Content.ReadFromJsonAsync<RentalDetailResponse>(JsonOptions);
        detail1!.InventoryId.Should().Be(9002);

        // Second create should get 9003 (next available)
        var response2 = await _client.PostAsJsonAsync("/api/rentals", request);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);
        var detail2 = await response2.Content.ReadFromJsonAsync<RentalDetailResponse>(JsonOptions);
        detail2!.InventoryId.Should().Be(9003);
    }

    [Fact]
    public async Task CreateRental_Appears_In_Get_List()
    {
        var request = new CreateRentalRequest
        {
            FilmId = 9001,
            StoreId = 9001,
            CustomerId = 9002,
            StaffId = 9001
        };

        var createResponse = await _client.PostAsJsonAsync("/api/rentals", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<RentalDetailResponse>(JsonOptions);

        var listResponse = await _client.GetAsync($"/api/rentals?customerId=9002");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await listResponse.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.Items.Should().Contain(r => r.Id == created!.Id);
    }

    // =====================================================
    // US2: GET /api/rentals — List and Filter Rentals
    // =====================================================

    [Fact]
    public async Task GetRentals_Returns_Paginated_With_Lean_DTO()
    {
        var response = await _client.GetAsync("/api/rentals");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(3);

        // Lean DTO: verify it has IDs, not names
        var item = result.Items[0];
        item.Id.Should().BeGreaterThan(0);
        item.InventoryId.Should().BeGreaterThan(0);
        item.CustomerId.Should().BeGreaterThan(0);
        item.StaffId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetRentals_FilterByCustomerId_Returns_Only_Customer_Rentals()
    {
        var response = await _client.GetAsync("/api/rentals?customerId=9001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(r => r.CustomerId == 9001);
    }

    [Fact]
    public async Task GetRentals_ActiveOnly_Returns_Only_Active_Rentals()
    {
        var response = await _client.GetAsync("/api/rentals?activeOnly=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(r => r.ReturnDate == null);
    }

    [Fact]
    public async Task GetRentals_ActiveOnly_False_Returns_All_Rentals()
    {
        var response = await _client.GetAsync("/api/rentals?activeOnly=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.TotalCount.Should().BeGreaterThanOrEqualTo(3); // mix of active and returned
    }

    [Fact]
    public async Task GetRentals_Combined_CustomerId_And_ActiveOnly()
    {
        // Customer 9001 has 2 active rentals (9001, 9003) and 0 returned
        var response = await _client.GetAsync("/api/rentals?customerId=9001&activeOnly=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(r => r.CustomerId == 9001 && r.ReturnDate == null);
    }

    [Fact]
    public async Task GetRentals_Pagination_Page2()
    {
        var response = await _client.GetAsync("/api/rentals?page=2&pageSize=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.Page.Should().Be(2);
        result.PageSize.Should().Be(1);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(3);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetRentals_NonExistent_Customer_Returns_Empty()
    {
        var response = await _client.GetAsync("/api/rentals?customerId=99999");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetRentals_Page_Less_Than_1_Returns_400()
    {
        var response = await _client.GetAsync("/api/rentals?page=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRentals_PageSize_Less_Than_1_Returns_400()
    {
        var response = await _client.GetAsync("/api/rentals?pageSize=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRentals_PageSize_Over_100_Capped_Silently()
    {
        var response = await _client.GetAsync("/api/rentals?pageSize=200");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetRentals_Page_Exceeding_Total_Returns_Empty_Items()
    {
        var response = await _client.GetAsync("/api/rentals?page=9999&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
        result.TotalPages.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetRentals_Default_Sort_Descending_By_RentalDate()
    {
        var response = await _client.GetAsync("/api/rentals?pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(2);

        for (var i = 0; i < result.Items.Count - 1; i++)
        {
            result.Items[i].RentalDate.Should().BeOnOrAfter(result.Items[i + 1].RentalDate);
        }
    }

    // =====================================================
    // US3: GET /api/rentals/{id} — View Rental Details
    // =====================================================

    [Fact]
    public async Task GetRentalById_Returns_Detail_Response()
    {
        var response = await _client.GetAsync("/api/rentals/9001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await response.Content.ReadFromJsonAsync<RentalDetailResponse>(JsonOptions);
        detail.Should().NotBeNull();
        detail!.Id.Should().Be(9001);
        detail.InventoryId.Should().Be(9001);
        detail.FilmId.Should().Be(9001);
        detail.FilmTitle.Should().Be("Rental Test Film A");
        detail.StoreId.Should().Be(9001);
        detail.CustomerId.Should().Be(9001);
        detail.CustomerFirstName.Should().Be("Alice");
        detail.CustomerLastName.Should().Be("Anderson");
        detail.StaffId.Should().Be(9001);
        detail.StaffFirstName.Should().Be("Active");
        detail.StaffLastName.Should().Be("Staffer");
    }

    [Fact]
    public async Task GetRentalById_Active_Rental_Has_Null_ReturnDate()
    {
        var response = await _client.GetAsync("/api/rentals/9001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await response.Content.ReadFromJsonAsync<RentalDetailResponse>(JsonOptions);
        detail!.ReturnDate.Should().BeNull();
    }

    [Fact]
    public async Task GetRentalById_Returned_Rental_Has_ReturnDate()
    {
        var response = await _client.GetAsync("/api/rentals/9002");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await response.Content.ReadFromJsonAsync<RentalDetailResponse>(JsonOptions);
        detail!.ReturnDate.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRentalById_NotFound_Returns_404()
    {
        var response = await _client.GetAsync("/api/rentals/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // =====================================================
    // US4: PUT /api/rentals/{id}/return — Return a Rental
    // =====================================================

    [Fact]
    public async Task ReturnRental_Active_Returns_200_With_ReturnDate()
    {
        var response = await _client.PutAsync("/api/rentals/9001/return", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await response.Content.ReadFromJsonAsync<RentalDetailResponse>(JsonOptions);
        detail.Should().NotBeNull();
        detail!.Id.Should().Be(9001);
        detail.ReturnDate.Should().NotBeNull();
        detail.ReturnDate!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
        detail.FilmTitle.Should().Be("Rental Test Film A");
        detail.CustomerFirstName.Should().Be("Alice");
        detail.StaffFirstName.Should().Be("Active");
    }

    [Fact]
    public async Task ReturnRental_Already_Returned_Returns_400()
    {
        // Rental 9002 is already returned
        var response = await _client.PutAsync("/api/rentals/9002/return", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Rental with ID 9002 has already been returned");
    }

    [Fact]
    public async Task ReturnRental_NotFound_Returns_404()
    {
        var response = await _client.PutAsync("/api/rentals/99999/return", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReturnRental_Makes_Inventory_Available()
    {
        // Return rental 9003 (Film B, inventory 9005)
        var returnResponse = await _client.PutAsync("/api/rentals/9003/return", null);
        returnResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Now create a new rental for Film B at store 1 — should succeed with inventory 9005
        var request = new CreateRentalRequest
        {
            FilmId = 9002,
            StoreId = 9001,
            CustomerId = 9001,
            StaffId = 9001
        };
        var createResponse = await _client.PostAsJsonAsync("/api/rentals", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var detail = await createResponse.Content.ReadFromJsonAsync<RentalDetailResponse>(JsonOptions);
        detail!.InventoryId.Should().Be(9005); // lowest available after return
    }

    [Fact]
    public async Task ReturnRental_Get_Shows_ReturnDate_Set()
    {
        await _client.PutAsync("/api/rentals/9001/return", null);

        var response = await _client.GetAsync("/api/rentals/9001");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await response.Content.ReadFromJsonAsync<RentalDetailResponse>(JsonOptions);
        detail!.ReturnDate.Should().NotBeNull();
    }

    // =====================================================
    // US5: DELETE /api/rentals/{id} — Delete a Rental
    // =====================================================

    [Fact]
    public async Task DeleteRental_NoPayments_Returns_204()
    {
        // Rental 9002 has no payments (returned rental)
        var response = await _client.DeleteAsync("/api/rentals/9002");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteRental_WithPayments_Returns_409()
    {
        // Seed a rental with payment
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await RentalTestHelper.SeedRentalWithPaymentAsync(db);

        var response = await _client.DeleteAsync("/api/rentals/9100");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Cannot delete rental with ID 9100 because it has associated payment records");
    }

    [Fact]
    public async Task DeleteRental_NotFound_Returns_404()
    {
        var response = await _client.DeleteAsync("/api/rentals/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRental_Then_Get_Returns_404()
    {
        var deleteResponse = await _client.DeleteAsync("/api/rentals/9002");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync("/api/rentals/9002");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRental_Then_List_Excludes_Deleted()
    {
        var deleteResponse = await _client.DeleteAsync("/api/rentals/9002");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await _client.GetAsync("/api/rentals?pageSize=100");
        var result = await listResponse.Content.ReadFromJsonAsync<PagedResponse<RentalListResponse>>(JsonOptions);
        result!.Items.Should().NotContain(r => r.Id == 9002);
    }
}
