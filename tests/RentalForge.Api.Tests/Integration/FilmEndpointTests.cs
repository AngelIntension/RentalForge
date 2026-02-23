using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Models;
using RentalForge.Api.Tests.Infrastructure;

namespace RentalForge.Api.Tests.Integration;

public class FilmEndpointTests : IClassFixture<TestWebAppFactory>, IAsyncLifetime
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public FilmEndpointTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();

        // Clean up test data via raw SQL to handle circular FKs
        await db.Database.ExecuteSqlRawAsync(
            """
            SET session_replication_role = 'replica';
            DELETE FROM inventory WHERE inventory_id >= 9000;
            DELETE FROM film_category WHERE film_id >= 9000;
            DELETE FROM film_actor WHERE film_id >= 9000;
            DELETE FROM film WHERE film_id >= 9000;
            DELETE FROM actor WHERE actor_id >= 9000;
            DELETE FROM category WHERE category_id >= 9000;
            DELETE FROM language WHERE language_id >= 9000;
            DELETE FROM staff WHERE staff_id >= 9500;
            DELETE FROM store WHERE store_id >= 9500;
            DELETE FROM address WHERE address_id >= 9500;
            SET session_replication_role = 'origin';
            """);

        await FilmTestHelper.SeedTestDataAsync(db);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // =====================================================
    // US1: GET /api/films — Browse and Search
    // =====================================================

    [Fact]
    public async Task GetFilms_Returns_Paginated_Films_With_Lean_DTO()
    {
        var response = await _client.GetAsync("/api/films");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetFilms_SearchByTitle_Returns_Filtered_Results()
    {
        var response = await _client.GetAsync("/api/films?search=alpha");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Alpha Adventure");
    }

    [Fact]
    public async Task GetFilms_SearchByDescription_Returns_Filtered_Results()
    {
        var response = await _client.GetAsync("/api/films?search=hilarious");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Beta Bonanza");
    }

    [Fact]
    public async Task GetFilms_SearchByActorName_Returns_Filtered_Results()
    {
        // Bob Wilson is in Beta Bonanza, Delta Drama, and Echo Explosion
        var response = await _client.GetAsync("/api/films?search=wilson");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetFilms_SearchIsCaseInsensitive()
    {
        var response = await _client.GetAsync("/api/films?search=ALPHA");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Alpha Adventure");
    }

    [Fact]
    public async Task GetFilms_FilterByCategory_Returns_Filtered_Results()
    {
        // Action films: Alpha Adventure, Charlie Chase, Echo Explosion
        var response = await _client.GetAsync("/api/films?category=Action");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetFilms_FilterByCategory_IsCaseInsensitive()
    {
        var response = await _client.GetAsync("/api/films?category=action");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetFilms_FilterByRating_Returns_Filtered_Results()
    {
        // PG films: Alpha Adventure
        var response = await _client.GetAsync("/api/films?rating=PG");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Alpha Adventure");
    }

    [Fact]
    public async Task GetFilms_FilterByYearRange_Returns_Filtered_Results()
    {
        // Films from 2020 to 2022: Alpha Adventure (2020), Charlie Chase (2022)
        var response = await _client.GetAsync("/api/films?yearFrom=2020&yearTo=2022");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFilms_FilterByYearFromOnly_Returns_Filtered_Results()
    {
        // Films from 2022+: Charlie Chase (2022), Echo Explosion (2023)
        var response = await _client.GetAsync("/api/films?yearFrom=2022");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFilms_FilterByYearToOnly_Returns_Filtered_Results()
    {
        // Films up to 2018: Beta Bonanza (2018), Delta Drama (2015)
        var response = await _client.GetAsync("/api/films?yearTo=2018");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFilms_CombinedFilters_UseAndLogic()
    {
        // Action films with search "chase": Charlie Chase
        var response = await _client.GetAsync("/api/films?search=chase&category=Action");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Charlie Chase");
    }

    [Fact]
    public async Task GetFilms_Pagination_Returns_CorrectMetadata()
    {
        var response = await _client.GetAsync("/api/films?page=2&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(5);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(3);
        result.Items.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetFilms_EmptyResults_Returns_ZeroTotalCount()
    {
        var response = await _client.GetAsync("/api/films?search=zzzznotfound");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetFilms_PageLessThan1_Returns400()
    {
        var response = await _client.GetAsync("/api/films?page=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetFilms_PageSizeLessThan1_Returns400()
    {
        var response = await _client.GetAsync("/api/films?pageSize=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetFilms_PageSizeAbove100_CappedSilently()
    {
        var response = await _client.GetAsync("/api/films?pageSize=200");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetFilms_YearFromGreaterThanYearTo_Returns400()
    {
        var response = await _client.GetAsync("/api/films?yearFrom=2025&yearTo=2020");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetFilms_DefaultOrder_AlphabeticalByTitle()
    {
        var response = await _client.GetAsync("/api/films");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(2);

        var titles = result.Items.Select(f => f.Title).ToList();
        titles.Should().BeInAscendingOrder();
    }

    // =====================================================
    // US2: GET /api/films/{id} — View Film Details
    // =====================================================

    [Fact]
    public async Task GetFilmById_ExistingFilm_Returns200WithDetailResponse()
    {
        var response = await _client.GetAsync("/api/films/9001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var film = await response.Content.ReadFromJsonAsync<FilmDetailResponse>(JsonOptions);
        film.Should().NotBeNull();
        film!.Id.Should().Be(9001);
        film.Title.Should().Be("Alpha Adventure");
        film.Description.Should().Be("An exciting action adventure film");
        film.ReleaseYear.Should().Be(2020);
        film.LanguageId.Should().Be(9001);
        film.LanguageName.Should().Be("English");
        film.OriginalLanguageId.Should().BeNull();
        film.OriginalLanguageName.Should().BeNull();
        film.RentalDuration.Should().Be(5);
        film.RentalRate.Should().Be(3.99m);
        film.Length.Should().Be(120);
        film.ReplacementCost.Should().Be(24.99m);
        film.Rating.Should().Be(MpaaRating.PG);
        film.SpecialFeatures.Should().Contain("Trailers");
        film.Actors.Should().HaveCount(2);
        film.Actors.Should().Contain("John Smith");
        film.Actors.Should().Contain("Jane Doe");
        film.Categories.Should().ContainSingle("Action");
    }

    [Fact]
    public async Task GetFilmById_FilmWithOriginalLanguage_ReturnsLanguageNames()
    {
        var response = await _client.GetAsync("/api/films/9002");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var film = await response.Content.ReadFromJsonAsync<FilmDetailResponse>(JsonOptions);
        film.Should().NotBeNull();
        film!.LanguageName.Should().Be("English");
        film.OriginalLanguageId.Should().Be(9002);
        film.OriginalLanguageName.Should().Be("French");
    }

    [Fact]
    public async Task GetFilmById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync("/api/films/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // =====================================================
    // US3: POST /api/films — Create Film
    // =====================================================

    [Fact]
    public async Task CreateFilm_RequiredFieldsOnly_Returns201WithLocationHeader()
    {
        var request = new CreateFilmRequest
        {
            Title = "New Test Film",
            LanguageId = 9001,
            RentalDuration = 5,
            RentalRate = 3.99m,
            ReplacementCost = 24.99m
        };

        var response = await _client.PostAsJsonAsync("/api/films", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var film = await response.Content.ReadFromJsonAsync<FilmDetailResponse>(JsonOptions);
        film.Should().NotBeNull();
        film!.Id.Should().BeGreaterThan(0);
        film.Title.Should().Be("New Test Film");
        film.LanguageName.Should().Be("English");
    }

    [Fact]
    public async Task CreateFilm_AllFields_Returns201()
    {
        var request = new CreateFilmRequest
        {
            Title = "Full Feature Film",
            Description = "A fully featured test film",
            ReleaseYear = 2025,
            LanguageId = 9001,
            OriginalLanguageId = 9002,
            RentalDuration = 7,
            RentalRate = 5.99m,
            Length = 150,
            ReplacementCost = 34.99m,
            Rating = MpaaRating.Pg13,
            SpecialFeatures = ["Trailers", "Commentaries"]
        };

        var response = await _client.PostAsJsonAsync("/api/films", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var film = await response.Content.ReadFromJsonAsync<FilmDetailResponse>(JsonOptions);
        film.Should().NotBeNull();
        film!.Title.Should().Be("Full Feature Film");
        film.Description.Should().Be("A fully featured test film");
        film.ReleaseYear.Should().Be(2025);
        film.OriginalLanguageName.Should().Be("French");
        film.Rating.Should().Be(MpaaRating.Pg13);
        film.SpecialFeatures.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateFilm_MissingTitle_Returns400()
    {
        var request = new CreateFilmRequest
        {
            Title = "",
            LanguageId = 9001,
            RentalDuration = 5,
            RentalRate = 3.99m,
            ReplacementCost = 24.99m
        };

        var response = await _client.PostAsJsonAsync("/api/films", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateFilm_InvalidLanguageId_Returns400WithMessage()
    {
        var request = new CreateFilmRequest
        {
            Title = "Bad Language Film",
            LanguageId = 99999,
            RentalDuration = 5,
            RentalRate = 3.99m,
            ReplacementCost = 24.99m
        };

        var response = await _client.PostAsJsonAsync("/api/films", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Language with ID 99999 does not exist.");
    }

    [Fact]
    public async Task CreateFilm_InvalidOriginalLanguageId_Returns400WithMessage()
    {
        var request = new CreateFilmRequest
        {
            Title = "Bad Original Language Film",
            LanguageId = 9001,
            OriginalLanguageId = 99999,
            RentalDuration = 5,
            RentalRate = 3.99m,
            ReplacementCost = 24.99m
        };

        var response = await _client.PostAsJsonAsync("/api/films", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Original language with ID 99999 does not exist.");
    }

    [Fact]
    public async Task CreateFilm_MultipleValidationErrors_AggregatedInSingleResponse()
    {
        var request = new CreateFilmRequest
        {
            Title = "",
            LanguageId = 99999,
            RentalDuration = 0,
            RentalRate = 0,
            ReplacementCost = 0
        };

        var response = await _client.PostAsJsonAsync("/api/films", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var problem = JsonDocument.Parse(content).RootElement;
        var errors = problem.GetProperty("errors");

        // Should have multiple errors aggregated (not early-return on first failure)
        errors.TryGetProperty("Title", out _).Should().BeTrue();
        errors.TryGetProperty("RentalDuration", out _).Should().BeTrue();
        errors.TryGetProperty("RentalRate", out _).Should().BeTrue();
        errors.TryGetProperty("ReplacementCost", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateFilm_RatingAcceptsStringValue()
    {
        // Send rating as string "PG-13" in raw JSON
        var json = """
        {
            "title": "String Rating Film",
            "languageId": 9001,
            "rentalDuration": 5,
            "rentalRate": 3.99,
            "replacementCost": 24.99,
            "rating": "PG-13"
        }
        """;

        var response = await _client.PostAsync("/api/films",
            new StringContent(json, Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var film = await response.Content.ReadFromJsonAsync<FilmDetailResponse>(JsonOptions);
        film.Should().NotBeNull();
        film!.Rating.Should().Be(MpaaRating.Pg13);
    }

    [Fact]
    public async Task CreateFilm_RatingAcceptsNumericValue()
    {
        // Send rating as numeric 2 (PG-13 = Pg13 = index 2) in raw JSON
        var json = """
        {
            "title": "Numeric Rating Film",
            "languageId": 9001,
            "rentalDuration": 5,
            "rentalRate": 3.99,
            "replacementCost": 24.99,
            "rating": 2
        }
        """;

        var response = await _client.PostAsync("/api/films",
            new StringContent(json, Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var film = await response.Content.ReadFromJsonAsync<FilmDetailResponse>(JsonOptions);
        film.Should().NotBeNull();
        film!.Rating.Should().Be(MpaaRating.Pg13);
    }

    [Fact]
    public async Task CreateFilm_AppearsInGetList()
    {
        var request = new CreateFilmRequest
        {
            Title = "Searchable Film XYZ",
            LanguageId = 9001,
            RentalDuration = 5,
            RentalRate = 3.99m,
            ReplacementCost = 24.99m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/films", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await _client.GetAsync("/api/films?search=Searchable Film XYZ");
        var result = await listResponse.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle(f => f.Title == "Searchable Film XYZ");
    }

    // =====================================================
    // US4: PUT /api/films/{id} — Update Film
    // =====================================================

    [Fact]
    public async Task UpdateFilm_ValidUpdate_Returns200WithUpdatedData()
    {
        var request = new UpdateFilmRequest
        {
            Title = "Alpha Updated",
            RentalDuration = 10,
            RentalRate = 9.99m,
            LanguageId = 9001,
            ReplacementCost = 49.99m
        };

        var response = await _client.PutAsJsonAsync("/api/films/9001", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var film = await response.Content.ReadFromJsonAsync<FilmDetailResponse>(JsonOptions);
        film.Should().NotBeNull();
        film!.Id.Should().Be(9001);
        film.Title.Should().Be("Alpha Updated");
        film.RentalRate.Should().Be(9.99m);
        film.ReplacementCost.Should().Be(49.99m);
    }

    [Fact]
    public async Task UpdateFilm_AllFields_Returns200()
    {
        var request = new UpdateFilmRequest
        {
            Title = "Fully Updated Film",
            Description = "Updated description",
            ReleaseYear = 2025,
            LanguageId = 9002,
            OriginalLanguageId = 9001,
            RentalDuration = 10,
            RentalRate = 9.99m,
            Length = 200,
            ReplacementCost = 49.99m,
            Rating = MpaaRating.R,
            SpecialFeatures = ["Director's Cut"]
        };

        var response = await _client.PutAsJsonAsync("/api/films/9001", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var film = await response.Content.ReadFromJsonAsync<FilmDetailResponse>(JsonOptions);
        film.Should().NotBeNull();
        film!.Title.Should().Be("Fully Updated Film");
        film.Description.Should().Be("Updated description");
        film.ReleaseYear.Should().Be(2025);
        film.LanguageName.Should().Be("French");
        film.OriginalLanguageName.Should().Be("English");
        film.Rating.Should().Be(MpaaRating.R);
    }

    [Fact]
    public async Task UpdateFilm_ValidationErrors_Returns400()
    {
        var request = new UpdateFilmRequest
        {
            Title = "",
            LanguageId = 9001,
            RentalDuration = 0,
            RentalRate = -1,
            ReplacementCost = 24.99m
        };

        var response = await _client.PutAsJsonAsync("/api/films/9001", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateFilm_NonExistentFilm_Returns404()
    {
        var request = new UpdateFilmRequest
        {
            Title = "Ghost Film",
            LanguageId = 9001,
            RentalDuration = 5,
            RentalRate = 3.99m,
            ReplacementCost = 24.99m
        };

        var response = await _client.PutAsJsonAsync("/api/films/99999", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateFilm_InvalidLanguageId_Returns400()
    {
        var request = new UpdateFilmRequest
        {
            Title = "Bad Language Update",
            LanguageId = 99999,
            RentalDuration = 5,
            RentalRate = 3.99m,
            ReplacementCost = 24.99m
        };

        var response = await _client.PutAsJsonAsync("/api/films/9001", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Language with ID 99999 does not exist.");
    }

    [Fact]
    public async Task UpdateFilm_MultipleValidationErrors_Aggregated()
    {
        var request = new UpdateFilmRequest
        {
            Title = "",
            LanguageId = 99999,
            RentalDuration = 0,
            RentalRate = 0,
            ReplacementCost = 0
        };

        var response = await _client.PutAsJsonAsync("/api/films/9001", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var problem = JsonDocument.Parse(content).RootElement;
        var errors = problem.GetProperty("errors");
        errors.TryGetProperty("Title", out _).Should().BeTrue();
        errors.TryGetProperty("RentalDuration", out _).Should().BeTrue();
    }

    // =====================================================
    // US5: DELETE /api/films/{id} — Remove Film
    // =====================================================

    [Fact]
    public async Task DeleteFilm_NoInventory_Returns204()
    {
        // Film 9005 has no inventory
        var response = await _client.DeleteAsync("/api/films/9005");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteFilm_JoinRowsAlsoRemoved()
    {
        // Film 9005 has film_actor and film_category rows
        var deleteResponse = await _client.DeleteAsync("/api/films/9005");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify film is gone
        var getResponse = await _client.GetAsync("/api/films/9005");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteFilm_WithInventory_Returns409Conflict()
    {
        // Seed a film with inventory
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DvdrentalContext>();
        await FilmTestHelper.SeedFilmWithInventoryAsync(db);

        var response = await _client.DeleteAsync("/api/films/9100");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Cannot delete film with ID 9100 because it has associated inventory records.");
    }

    [Fact]
    public async Task DeleteFilm_NonExistent_Returns404()
    {
        var response = await _client.DeleteAsync("/api/films/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteFilm_ExcludedFromGetList()
    {
        var deleteResponse = await _client.DeleteAsync("/api/films/9004");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await _client.GetAsync("/api/films?search=Delta Drama");
        var result = await listResponse.Content.ReadFromJsonAsync<PagedResponse<FilmListResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteFilm_Returns404OnGetById()
    {
        var deleteResponse = await _client.DeleteAsync("/api/films/9004");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync("/api/films/9004");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
