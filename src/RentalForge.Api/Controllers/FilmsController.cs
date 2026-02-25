using Ardalis.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalForge.Api.Models;
using RentalForge.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace RentalForge.Api.Controllers;

/// <summary>
/// Manages film CRUD operations.
/// </summary>
[ApiController]
[Route("api/films")]
[Authorize]
public class FilmsController(IFilmService filmService) : ControllerBase
{
    /// <summary>
    /// Lists films with optional search, filtering, and pagination.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(OperationId = "ListFilms", Summary = "List films with search, filtering, and pagination")]
    [ProducesResponseType(typeof(PagedResponse<FilmListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFilms(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? rating = null,
        [FromQuery] int? yearFrom = null,
        [FromQuery] int? yearTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var errors = new Dictionary<string, string[]>();
        if (page < 1)
            errors["page"] = ["'Page' must be greater than or equal to '1'."];
        if (pageSize < 1)
            errors["pageSize"] = ["'Page Size' must be greater than or equal to '1'."];
        if (yearFrom.HasValue && yearTo.HasValue && yearFrom > yearTo)
            errors["yearFrom"] = ["'Year From' must be less than or equal to 'Year To'."];
        if (errors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(errors));

        pageSize = Math.Min(pageSize, 100);
        var result = await filmService.GetFilmsAsync(search, category, rating, yearFrom, yearTo, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gets full film details by ID, including actors, categories, and language names.
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(OperationId = "GetFilm", Summary = "Get film by ID with full details")]
    [ProducesResponseType(typeof(FilmDetailResponse), StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Film not found")]
    public async Task<IActionResult> GetFilm(int id)
    {
        var result = await filmService.GetFilmByIdAsync(id);
        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Creates a new film.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "CreateFilm", Summary = "Create a new film")]
    [ProducesResponseType(typeof(FilmDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFilm([FromBody] CreateFilmRequest request)
    {
        var result = await filmService.CreateFilmAsync(request);
        return result.Status switch
        {
            ResultStatus.Created => CreatedAtAction(nameof(GetFilm), new { id = result.Value.Id }, result.Value),
            ResultStatus.Invalid => InvalidResult(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Updates an existing film (full replacement).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "UpdateFilm", Summary = "Update film by ID")]
    [ProducesResponseType(typeof(FilmDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Film not found")]
    public async Task<IActionResult> UpdateFilm(int id, [FromBody] UpdateFilmRequest request)
    {
        var result = await filmService.UpdateFilmAsync(id, request);
        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Invalid => InvalidResult(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Permanently deletes a film (hard delete). Films with inventory records cannot be deleted.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "DeleteFilm", Summary = "Delete film by ID")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Film not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Film has associated inventory records")]
    public async Task<IActionResult> DeleteFilm(int id)
    {
        var result = await filmService.DeleteFilmAsync(id);
        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Conflict => Conflict(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = result.Errors.FirstOrDefault()
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private IActionResult InvalidResult(IEnumerable<ValidationError> errors)
    {
        foreach (var error in errors)
            ModelState.AddModelError(error.Identifier, error.ErrorMessage);
        return ValidationProblem(ModelState);
    }
}
