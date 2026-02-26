using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Models;

namespace RentalForge.Api.Services;

public class RentalService(
    DvdrentalContext db,
    ILogger<RentalService> logger,
    IValidator<CreateRentalRequest> createValidator,
    IValidator<ReturnRentalRequest> returnValidator) : IRentalService
{
    public async Task<PagedResponse<RentalListResponse>> GetRentalsAsync(
        int? customerId, bool activeOnly, int page, int pageSize)
    {
        var query = db.Rentals.AsQueryable();

        if (customerId.HasValue)
            query = query.Where(r => r.CustomerId == customerId.Value);

        if (activeOnly)
            query = query.Where(r => r.ReturnDate == null);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.RentalDate)
            .ThenByDescending(r => r.RentalId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RentalListResponse(
                r.RentalId,
                r.RentalDate,
                r.ReturnDate,
                r.InventoryId,
                r.CustomerId,
                r.StaffId,
                r.LastUpdate,
                r.Payments.Sum(p => p.Amount),
                r.Inventory.Film.RentalRate,
                r.Inventory.Film.RentalRate - r.Payments.Sum(p => p.Amount)))
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResponse<RentalListResponse>(items, page, pageSize, totalCount, totalPages);
    }

    public async Task<Result<RentalDetailResponse>> GetRentalByIdAsync(int id)
    {
        var rental = await db.Rentals
            .Where(r => r.RentalId == id)
            .Select(r => new RentalDetailResponse(
                r.RentalId,
                r.RentalDate,
                r.ReturnDate,
                r.InventoryId,
                r.Inventory.FilmId,
                r.Inventory.Film.Title,
                r.Inventory.StoreId,
                r.CustomerId,
                r.Customer.FirstName,
                r.Customer.LastName,
                r.StaffId,
                r.Staff.FirstName,
                r.Staff.LastName,
                r.LastUpdate,
                r.Payments.Sum(p => p.Amount),
                r.Inventory.Film.RentalRate,
                r.Inventory.Film.RentalRate - r.Payments.Sum(p => p.Amount))
            {
                Payments = r.Payments
                    .OrderBy(p => p.PaymentDate)
                    .Select(p => new RentalPaymentItem(
                        p.PaymentId,
                        p.Amount,
                        p.PaymentDate,
                        p.StaffId))
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return rental is not null
            ? Result<RentalDetailResponse>.Success(rental)
            : Result<RentalDetailResponse>.NotFound();
    }

    public async Task<Result<RentalDetailResponse>> CreateRentalAsync(CreateRentalRequest request)
    {
        var validationResult = await createValidator.ValidateAsync(request);
        var allErrors = validationResult.AsErrors();

        if (!await db.Films.AnyAsync(f => f.FilmId == request.FilmId))
            allErrors.Add(new ValidationError("filmId", $"Film with ID {request.FilmId} does not exist."));

        if (!await db.Stores.AnyAsync(s => s.StoreId == request.StoreId))
            allErrors.Add(new ValidationError("storeId", $"Store with ID {request.StoreId} does not exist."));

        if (!await db.Customers.AnyAsync(c => c.CustomerId == request.CustomerId && c.Activebool))
            allErrors.Add(new ValidationError("customerId", $"Customer with ID {request.CustomerId} does not exist or is inactive."));

        if (!await db.Staff.AnyAsync(s => s.StaffId == request.StaffId && s.Active))
            allErrors.Add(new ValidationError("staffId", $"Staff member with ID {request.StaffId} does not exist or is inactive."));

        if (allErrors.Count > 0)
            return Result<RentalDetailResponse>.Invalid(allErrors);

        // Find available inventory: filmId + storeId, no active rental, ordered by InventoryId
        var inventoryQuery = db.Inventories
            .Where(i => i.FilmId == request.FilmId && i.StoreId == request.StoreId);

        var anyInventory = await inventoryQuery.AnyAsync();
        if (!anyInventory)
        {
            var filmTitle = await db.Films.Where(f => f.FilmId == request.FilmId).Select(f => f.Title).FirstAsync();
            allErrors.Add(new ValidationError("filmId", $"Film '{filmTitle}' is not stocked at store {request.StoreId}."));
            return Result<RentalDetailResponse>.Invalid(allErrors);
        }

        var availableInventory = await inventoryQuery
            .Include(i => i.Film)
            .Where(i => !i.Rentals.Any(r => r.ReturnDate == null))
            .OrderBy(i => i.InventoryId)
            .FirstOrDefaultAsync();

        if (availableInventory is null)
        {
            var filmTitle = await db.Films.Where(f => f.FilmId == request.FilmId).Select(f => f.Title).FirstAsync();
            allErrors.Add(new ValidationError("filmId", $"All copies of film '{filmTitle}' at store {request.StoreId} are currently rented out."));
            return Result<RentalDetailResponse>.Invalid(allErrors);
        }

        var rental = new Rental
        {
            RentalDate = DateTime.UtcNow,
            InventoryId = availableInventory.InventoryId,
            CustomerId = request.CustomerId,
            StaffId = request.StaffId,
            LastUpdate = DateTime.UtcNow
        };

        db.Rentals.Add(rental);
        await db.SaveChangesAsync();

        logger.LogInformation("Created rental {RentalId} for film '{FilmTitle}' (customer: {CustomerName})",
            rental.RentalId, availableInventory.Film?.Title ?? "Unknown",
            $"{request.CustomerId}");

        var detail = await GetRentalByIdAsync(rental.RentalId);
        return Result<RentalDetailResponse>.Created(detail.Value);
    }

    public async Task<Result<RentalDetailResponse>> ReturnRentalAsync(int id, ReturnRentalRequest? request = null)
    {
        var rental = await db.Rentals.FirstOrDefaultAsync(r => r.RentalId == id);

        if (rental is null)
            return Result<RentalDetailResponse>.NotFound();

        if (rental.ReturnDate is not null)
            return Result<RentalDetailResponse>.Invalid(
                new ValidationError("rentalId", $"Rental with ID {id} has already been returned."));

        // Validate and process optional payment
        if (request?.Amount is not null)
        {
            var validationResult = await returnValidator.ValidateAsync(request);
            var allErrors = validationResult.AsErrors();

            if (request.StaffId.HasValue && request.StaffId > 0 &&
                !await db.Staff.AnyAsync(s => s.StaffId == request.StaffId && s.Active))
                allErrors.Add(new ValidationError("staffId",
                    $"Staff member with ID {request.StaffId} does not exist or is inactive."));

            if (allErrors.Count > 0)
                return Result<RentalDetailResponse>.Invalid(allErrors);

            var payment = new Payment
            {
                CustomerId = rental.CustomerId,
                StaffId = request.StaffId!.Value,
                RentalId = rental.RentalId,
                Amount = request.Amount.Value,
                PaymentDate = DateTime.UtcNow
            };
            db.Payments.Add(payment);

            logger.LogInformation("Created payment {PaymentId} during return of rental {RentalId}",
                payment.PaymentId, rental.RentalId);
        }

        rental.ReturnDate = DateTime.UtcNow;
        rental.LastUpdate = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation("Returned rental {RentalId}", rental.RentalId);

        var detail = await GetRentalByIdAsync(rental.RentalId);
        return Result<RentalDetailResponse>.Success(detail.Value);
    }

    public async Task<Result> DeleteRentalAsync(int id)
    {
        var rental = await db.Rentals.FirstOrDefaultAsync(r => r.RentalId == id);

        if (rental is null)
            return Result.NotFound();

        if (await db.Payments.AnyAsync(p => p.RentalId == id))
        {
            logger.LogInformation("Delete blocked for rental {RentalId} — has associated payment records", id);
            return Result.Conflict($"Cannot delete rental with ID {id} because it has associated payment records.");
        }

        db.Rentals.Remove(rental);
        await db.SaveChangesAsync();

        logger.LogInformation("Deleted rental {RentalId}", id);

        return Result.NoContent();
    }
}
