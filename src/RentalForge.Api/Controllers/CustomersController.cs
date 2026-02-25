using System.Security.Claims;
using Ardalis.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalForge.Api.Models;
using RentalForge.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace RentalForge.Api.Controllers;

/// <summary>
/// Manages customer CRUD operations.
/// </summary>
[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    /// <summary>
    /// Lists active customers with optional search and pagination. Staff and Admin only.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "ListCustomers", Summary = "List active customers with search and pagination")]
    [ProducesResponseType(typeof(PagedResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var errors = new Dictionary<string, string[]>();
        if (page < 1)
            errors["page"] = ["'Page' must be greater than or equal to '1'."];
        if (pageSize < 1 || pageSize > 100)
            errors["pageSize"] = ["'Page Size' must be between 1 and 100."];
        if (errors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(errors));

        pageSize = Math.Min(pageSize, 100);
        var result = await customerService.GetCustomersAsync(search, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gets a single active customer by ID. Customer-role users can view their own record.
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(OperationId = "GetCustomer", Summary = "Get customer by ID")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Customer not found or deactivated")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        // Customer-role users can only view their own record
        if (User.IsInRole("Customer"))
        {
            var customerIdClaim = GetCurrentUserCustomerId();
            if (customerIdClaim is null || customerIdClaim != id)
                return Forbid();
        }

        var result = await customerService.GetCustomerByIdAsync(id);
        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Creates a new customer. Staff and Admin only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "CreateCustomer", Summary = "Create a new customer")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var result = await customerService.CreateCustomerAsync(request);
        return result.Status switch
        {
            ResultStatus.Created => CreatedAtAction(nameof(GetCustomer), new { id = result.Value.Id }, result.Value),
            ResultStatus.Invalid => InvalidResult(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Updates an existing active customer (full replacement). Staff and Admin only.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "UpdateCustomer", Summary = "Update customer by ID")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Customer not found or deactivated")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
    {
        var result = await customerService.UpdateCustomerAsync(id, request);
        return result.Status switch
        {
            ResultStatus.Ok => Ok(result.Value),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Invalid => InvalidResult(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Soft-deletes (deactivates) an active customer. Staff and Admin only.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "DeactivateCustomer", Summary = "Deactivate customer by ID")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Customer not found or already deactivated")]
    public async Task<IActionResult> DeactivateCustomer(int id)
    {
        var result = await customerService.DeactivateCustomerAsync(id);
        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(),
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
