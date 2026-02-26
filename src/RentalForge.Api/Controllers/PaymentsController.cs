using System.Security.Claims;
using Ardalis.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Models;
using RentalForge.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace RentalForge.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    [SwaggerOperation(OperationId = "CreatePayment", Summary = "Record a payment for a rental")]
    [ProducesResponseType(typeof(PaymentDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        var result = await paymentService.CreatePaymentAsync(request);
        return result.Status switch
        {
            ResultStatus.Created => Created($"/api/payments/{result.Value.Id}", result.Value),
            ResultStatus.Invalid => InvalidResult(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "ListPayments", Summary = "List payments with filtering and pagination")]
    [ProducesResponseType(typeof(PagedResponse<PaymentListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int? customerId = null,
        [FromQuery] int? staffId = null,
        [FromQuery] int? rentalId = null,
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

        int? storeId = null;

        if (User.IsInRole("Customer"))
        {
            var userCustomerId = GetCurrentUserCustomerId();
            if (userCustomerId is null)
                return Ok(new PagedResponse<PaymentListResponse>([], page, pageSize, 0, 0));
            customerId = userCustomerId;
        }
        else if (User.IsInRole("Staff") && !User.IsInRole("Admin"))
        {
            storeId = await GetCurrentUserStoreIdAsync();
        }

        pageSize = Math.Min(pageSize, 100);
        var result = await paymentService.GetPaymentsAsync(customerId, staffId, rentalId, storeId, page, pageSize);
        return Ok(result);
    }

    private int? GetCurrentUserCustomerId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (userId is null)
            return null;

        using var scope = HttpContext.RequestServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
        return user?.CustomerId;
    }

    private async Task<int?> GetCurrentUserStoreIdAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (userId is null)
            return null;

        using var scope = HttpContext.RequestServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);
        if (user?.StaffId is null)
            return null;

        var db = scope.ServiceProvider.GetRequiredService<Data.DvdrentalContext>();
        var staff = await db.Staff.FirstOrDefaultAsync(s => s.StaffId == user.StaffId);
        return staff?.StoreId;
    }

    private IActionResult InvalidResult(IEnumerable<ValidationError> errors)
    {
        foreach (var error in errors)
            ModelState.AddModelError(error.Identifier, error.ErrorMessage);
        return ValidationProblem(ModelState);
    }
}
