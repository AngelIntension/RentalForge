using System.Security.Claims;
using Ardalis.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalForge.Api.Models;
using RentalForge.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace RentalForge.Api.Controllers;

/// <summary>
/// Manages rental CRUD operations and rental returns.
/// </summary>
[ApiController]
[Route("api/rentals")]
[Authorize]
public class RentalsController(IRentalService rentalService) : ControllerBase
{
    /// <summary>
    /// Lists rentals with optional filtering by customer and active status, plus pagination.
    /// Customer-role users only see their own rentals.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(OperationId = "ListRentals", Summary = "List rentals with filtering and pagination")]
    [ProducesResponseType(typeof(PagedResponse<RentalListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRentals(
        [FromQuery] int? customerId = null,
        [FromQuery] bool activeOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var errors = new Dictionary<string, string[]>();
        if (page < 1)
            errors["page"] = ["'Page' must be greater than or equal to '1'."];
        if (pageSize < 1)
            errors["pageSize"] = ["'Page Size' must be greater than or equal to '1'."];
        if (errors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(errors));

        // Customer-role users can only see their own rentals
        if (User.IsInRole("Customer"))
        {
            var userCustomerId = GetCurrentUserCustomerId();
            if (userCustomerId is null)
                return Ok(new PagedResponse<RentalListResponse>([], page, pageSize, 0, 0));
            customerId = userCustomerId;
        }

        pageSize = Math.Min(pageSize, 100);
        var result = await rentalService.GetRentalsAsync(customerId, activeOnly, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gets full rental details by ID, including customer name, film title, and staff name.
    /// Customer-role users can only view their own rentals.
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(OperationId = "GetRental", Summary = "Get rental by ID with full details")]
    [ProducesResponseType(typeof(RentalDetailResponse), StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Rental not found")]
    public async Task<IActionResult> GetRental(int id)
    {
        var result = await rentalService.GetRentalByIdAsync(id);

        if (result.Status == ResultStatus.Ok && User.IsInRole("Customer"))
        {
            var userCustomerId = GetCurrentUserCustomerId();
            if (userCustomerId is null || result.Value.CustomerId != userCustomerId)
                return Forbid();
        }

        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Creates a new rental. Accepts filmId + storeId; the system resolves an available inventory copy.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "CreateRental", Summary = "Create a new rental with inventory resolution")]
    [ProducesResponseType(typeof(RentalDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRental([FromBody] CreateRentalRequest request)
    {
        var result = await rentalService.CreateRentalAsync(request);
        return result.Status switch
        {
            ResultStatus.Created => CreatedAtAction(nameof(GetRental), new { id = result.Value.Id }, result.Value),
            ResultStatus.Invalid => InvalidResult(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Processes a rental return. Sets the return date to the current timestamp.
    /// </summary>
    [HttpPut("{id:int}/return")]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "ReturnRental", Summary = "Process a rental return")]
    [ProducesResponseType(typeof(RentalDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Rental not found")]
    public async Task<IActionResult> ReturnRental(int id)
    {
        var result = await rentalService.ReturnRentalAsync(id);
        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Invalid => InvalidResult(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Permanently deletes a rental (hard delete). Rentals with payment records cannot be deleted.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "DeleteRental", Summary = "Delete rental by ID")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Rental not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Rental has associated payment records")]
    public async Task<IActionResult> DeleteRental(int id)
    {
        var result = await rentalService.DeleteRentalAsync(id);
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

    private int? GetCurrentUserCustomerId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (userId is null)
            return null;

        using var scope = HttpContext.RequestServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Data.Entities.ApplicationUser>>();
        var user = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
        return user?.CustomerId;
    }

    private IActionResult InvalidResult(IEnumerable<ValidationError> errors)
    {
        foreach (var error in errors)
            ModelState.AddModelError(error.Identifier, error.ErrorMessage);
        return ValidationProblem(ModelState);
    }
}
