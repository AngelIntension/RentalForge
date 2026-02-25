using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalForge.Api.Data;
using RentalForge.Api.Models;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

public class CustomerEndpointTests : IClassFixture<TestWebAppFactory>, IAsyncLifetime
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CustomerEndpointTests(TestWebAppFactory factory)
    {
        _factory = factory;
        // Use an authenticated Staff client for all customer endpoint tests
        _client = AuthTestHelper.CreateAuthenticatedClient(
            factory, "test-staff-id", "staff@test.com", "Staff");
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Clean up via raw SQL to handle Store↔Staff circular FK
        await db.Database.ExecuteSqlRawAsync(
            """
            SET session_replication_role = 'replica';
            DELETE FROM customer WHERE customer_id >= 9000;
            DELETE FROM staff WHERE staff_id >= 9000;
            DELETE FROM store WHERE store_id >= 9000;
            DELETE FROM address WHERE address_id >= 9000;
            SET session_replication_role = 'origin';
            """);

        await CustomerTestHelper.SeedTestDataAsync(db);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // =====================================================
    // US1: GET /api/customers — Browse and Search
    // =====================================================

    [Fact]
    public async Task GetCustomers_Returns_Paginated_Active_Customers()
    {
        var response = await _client.GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().AllSatisfy(c => c.IsActive.Should().BeTrue());
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2); // Alice and Bob are active
    }

    [Fact]
    public async Task GetCustomers_SearchByFirstName_Returns_Filtered_Results()
    {
        var response = await _client.GetAsync("/api/customers?search=alice");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetCustomers_SearchByLastName_Returns_Filtered_Results()
    {
        var response = await _client.GetAsync("/api/customers?search=brown");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].LastName.Should().Be("Brown");
    }

    [Fact]
    public async Task GetCustomers_SearchByEmail_Returns_Filtered_Results()
    {
        var response = await _client.GetAsync("/api/customers?search=alice@example");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task GetCustomers_SearchIsCaseInsensitive()
    {
        var response = await _client.GetAsync("/api/customers?search=ALICE");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetCustomers_SearchNoMatches_Returns_EmptyList()
    {
        var response = await _client.GetAsync("/api/customers?search=zzzzzznotfound");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetCustomers_Pagination_ReturnsCorrectMetadata()
    {
        // Seed more customers for pagination
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await CustomerTestHelper.SeedManyCustomersAsync(db, 15);

        var response = await _client.GetAsync("/api/customers?page=2&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.Items.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetCustomers_PageLessThan1_Returns400()
    {
        var response = await _client.GetAsync("/api/customers?page=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomers_BothPageAndPageSizeInvalid_ReturnsBothErrors()
    {
        var response = await _client.GetAsync("/api/customers?page=0&pageSize=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var problem = JsonDocument.Parse(content).RootElement;
        var errors = problem.GetProperty("errors");
        errors.TryGetProperty("page", out _).Should().BeTrue();
        errors.TryGetProperty("pageSize", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomers_ExcludesDeactivatedCustomers()
    {
        // Charlie (9003) is deactivated in test data
        var response = await _client.GetAsync("/api/customers?search=charlie");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCustomers_OrderedByLastNameThenFirstName()
    {
        var response = await _client.GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();

        var names = result!.Items.Select(c => c.LastName).ToList();
        names.Should().BeInAscendingOrder();
    }

    // =====================================================
    // US2: GET /api/customers/{id} — View Customer Details
    // =====================================================

    [Fact]
    public async Task GetCustomerById_ActiveCustomer_Returns200WithDetails()
    {
        var response = await _client.GetAsync("/api/customers/9001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(9001);
        customer.FirstName.Should().Be("Alice");
        customer.LastName.Should().Be("Anderson");
        customer.Email.Should().Be("alice@example.com");
        customer.StoreId.Should().Be(9001);
        customer.AddressId.Should().Be(9100);
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomerById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync("/api/customers/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCustomerById_DeactivatedCustomer_Returns404()
    {
        // Charlie (9003) is deactivated
        var response = await _client.GetAsync("/api/customers/9003");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // =====================================================
    // US3: POST /api/customers — Register a New Customer
    // =====================================================

    [Fact]
    public async Task CreateCustomer_ValidRequest_Returns201WithLocationHeader()
    {
        var request = new CreateCustomerRequest
        {
            FirstName = "Diana",
            LastName = "Davis",
            Email = "diana@example.com",
            StoreId = 9001,
            AddressId = 9100
        };

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        customer.Should().NotBeNull();
        customer!.Id.Should().BeGreaterThan(0);
        customer.FirstName.Should().Be("Diana");
        customer.LastName.Should().Be("Davis");
        customer.Email.Should().Be("diana@example.com");
        customer.StoreId.Should().Be(9001);
        customer.AddressId.Should().Be(9100);
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomer_MissingRequiredFields_Returns400()
    {
        var request = new CreateCustomerRequest
        {
            FirstName = "",
            LastName = "",
            StoreId = 0,
            AddressId = 0
        };

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var problem = JsonDocument.Parse(content).RootElement;
        var errors = problem.GetProperty("errors");
        errors.TryGetProperty("FirstName", out _).Should().BeTrue();
        errors.TryGetProperty("LastName", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomer_InvalidEmail_Returns400()
    {
        var request = new CreateCustomerRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "not-an-email",
            StoreId = 9001,
            AddressId = 9100
        };

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCustomer_NonExistentStoreId_Returns400()
    {
        var request = new CreateCustomerRequest
        {
            FirstName = "Test",
            LastName = "User",
            StoreId = 99999,
            AddressId = 9100
        };

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCustomer_NonExistentAddressId_Returns400()
    {
        var request = new CreateCustomerRequest
        {
            FirstName = "Test",
            LastName = "User",
            StoreId = 9001,
            AddressId = 99999
        };

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCustomer_BothStoreIdAndAddressIdInvalid_ReturnsBothErrors()
    {
        var request = new CreateCustomerRequest
        {
            FirstName = "Test",
            LastName = "User",
            StoreId = 99999,
            AddressId = 99999
        };

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var problem = JsonDocument.Parse(content).RootElement;
        var errors = problem.GetProperty("errors");
        errors.TryGetProperty("storeId", out _).Should().BeTrue();
        errors.TryGetProperty("addressId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomer_AppearsInGetList()
    {
        var request = new CreateCustomerRequest
        {
            FirstName = "Eva",
            LastName = "Evans",
            Email = "eva@example.com",
            StoreId = 9001,
            AddressId = 9101
        };

        var createResponse = await _client.PostAsJsonAsync("/api/customers", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await _client.GetAsync("/api/customers?search=eva");
        var result = await listResponse.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle(c => c.FirstName == "Eva");
    }

    // =====================================================
    // US4: PUT /api/customers/{id} — Update Customer
    // =====================================================

    [Fact]
    public async Task UpdateCustomer_ValidUpdate_Returns200WithUpdatedData()
    {
        var request = new UpdateCustomerRequest
        {
            FirstName = "AliceUpdated",
            LastName = "AndersonUpdated",
            Email = "alice.updated@example.com",
            StoreId = 9001,
            AddressId = 9100
        };

        var response = await _client.PutAsJsonAsync("/api/customers/9001", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(9001);
        customer.FirstName.Should().Be("AliceUpdated");
        customer.LastName.Should().Be("AndersonUpdated");
        customer.Email.Should().Be("alice.updated@example.com");
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCustomer_InvalidData_Returns400()
    {
        var request = new UpdateCustomerRequest
        {
            FirstName = "",
            LastName = "",
            StoreId = 0,
            AddressId = 0
        };

        var response = await _client.PutAsJsonAsync("/api/customers/9001", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCustomer_NonExistentCustomer_Returns404()
    {
        var request = new UpdateCustomerRequest
        {
            FirstName = "Test",
            LastName = "User",
            StoreId = 9001,
            AddressId = 9100
        };

        var response = await _client.PutAsJsonAsync("/api/customers/99999", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_DeactivatedCustomer_Returns404()
    {
        var request = new UpdateCustomerRequest
        {
            FirstName = "Test",
            LastName = "User",
            StoreId = 9001,
            AddressId = 9100
        };

        // Charlie (9003) is deactivated
        var response = await _client.PutAsJsonAsync("/api/customers/9003", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_BothStoreIdAndAddressIdInvalid_ReturnsBothErrors()
    {
        var request = new UpdateCustomerRequest
        {
            FirstName = "Test",
            LastName = "User",
            StoreId = 99999,
            AddressId = 99999
        };

        var response = await _client.PutAsJsonAsync("/api/customers/9001", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var problem = JsonDocument.Parse(content).RootElement;
        var errors = problem.GetProperty("errors");
        errors.TryGetProperty("storeId", out _).Should().BeTrue();
        errors.TryGetProperty("addressId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCustomer_ReflectsChangesInSubsequentGet()
    {
        var request = new UpdateCustomerRequest
        {
            FirstName = "BobUpdated",
            LastName = "BrownUpdated",
            Email = "bob.updated@example.com",
            StoreId = 9001,
            AddressId = 9101
        };

        var updateResponse = await _client.PutAsJsonAsync("/api/customers/9002", request);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync("/api/customers/9002");
        var customer = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        customer.Should().NotBeNull();
        customer!.FirstName.Should().Be("BobUpdated");
        customer.LastName.Should().Be("BrownUpdated");
    }

    // =====================================================
    // Swagger Metadata Verification
    // =====================================================

    [Fact]
    public async Task SwaggerMetadata_ContainsAllCustomerEndpoints()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var swagger = JsonDocument.Parse(content).RootElement;
        var paths = swagger.GetProperty("paths");

        // Verify all 5 customer endpoints exist
        paths.TryGetProperty("/api/customers", out var customersPath).Should().BeTrue();
        customersPath.TryGetProperty("get", out var listOp).Should().BeTrue();
        listOp.GetProperty("operationId").GetString().Should().Be("ListCustomers");

        customersPath.TryGetProperty("post", out var createOp).Should().BeTrue();
        createOp.GetProperty("operationId").GetString().Should().Be("CreateCustomer");

        paths.TryGetProperty("/api/customers/{id}", out var customerByIdPath).Should().BeTrue();
        customerByIdPath.TryGetProperty("get", out var getOp).Should().BeTrue();
        getOp.GetProperty("operationId").GetString().Should().Be("GetCustomer");

        customerByIdPath.TryGetProperty("put", out var updateOp).Should().BeTrue();
        updateOp.GetProperty("operationId").GetString().Should().Be("UpdateCustomer");

        customerByIdPath.TryGetProperty("delete", out var deleteOp).Should().BeTrue();
        deleteOp.GetProperty("operationId").GetString().Should().Be("DeactivateCustomer");
    }

    // =====================================================
    // US5: DELETE /api/customers/{id} — Deactivate Customer
    // =====================================================

    [Fact]
    public async Task DeactivateCustomer_ActiveCustomer_Returns204()
    {
        var response = await _client.DeleteAsync("/api/customers/9001");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeactivateCustomer_AlreadyDeactivated_Returns404()
    {
        // Charlie (9003) is already deactivated
        var response = await _client.DeleteAsync("/api/customers/9003");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateCustomer_NonExistent_Returns404()
    {
        var response = await _client.DeleteAsync("/api/customers/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateCustomer_ExcludedFromGetList()
    {
        // Deactivate Bob
        var deleteResponse = await _client.DeleteAsync("/api/customers/9002");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify Bob is excluded from list
        var listResponse = await _client.GetAsync("/api/customers?search=bob");
        var result = await listResponse.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task DeactivateCustomer_Returns404OnGetById()
    {
        // Deactivate Alice
        var deleteResponse = await _client.DeleteAsync("/api/customers/9001");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify Alice returns 404 on GET by ID
        var getResponse = await _client.GetAsync("/api/customers/9001");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
