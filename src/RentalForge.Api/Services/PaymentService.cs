using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Models;

namespace RentalForge.Api.Services;

public class PaymentService(
    DvdrentalContext db,
    ILogger<PaymentService> logger,
    IValidator<CreatePaymentRequest> createValidator) : IPaymentService
{
    public async Task<Result<PaymentDetailResponse>> CreatePaymentAsync(CreatePaymentRequest request)
    {
        var validationResult = await createValidator.ValidateAsync(request);
        var allErrors = validationResult.AsErrors();

        var rental = await db.Rentals.FirstOrDefaultAsync(r => r.RentalId == request.RentalId);
        if (request.RentalId > 0 && rental is null)
            allErrors.Add(new ValidationError("rentalId", $"Rental with ID {request.RentalId} was not found."));

        if (request.StaffId > 0 && !await db.Staff.AnyAsync(s => s.StaffId == request.StaffId && s.Active))
            allErrors.Add(new ValidationError("staffId", $"Staff member with ID {request.StaffId} does not exist or is inactive."));

        if (allErrors.Count > 0)
            return Result<PaymentDetailResponse>.Invalid(allErrors);

        var payment = new Payment
        {
            CustomerId = rental!.CustomerId,
            StaffId = request.StaffId,
            RentalId = request.RentalId,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate ?? DateTime.UtcNow
        };

        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        logger.LogInformation("Created payment {PaymentId} for rental {RentalId} (amount: {Amount})",
            payment.PaymentId, payment.RentalId, payment.Amount);

        var detail = await db.Payments
            .Where(p => p.PaymentId == payment.PaymentId)
            .Select(p => new PaymentDetailResponse(
                p.PaymentId,
                p.RentalId,
                p.CustomerId,
                p.Customer.FirstName,
                p.Customer.LastName,
                p.StaffId,
                p.Staff.FirstName,
                p.Staff.LastName,
                p.Amount,
                p.PaymentDate,
                p.Rental.Inventory.Film.Title))
            .FirstAsync();

        return Result<PaymentDetailResponse>.Created(detail);
    }

    public async Task<PagedResponse<PaymentListResponse>> GetPaymentsAsync(
        int? customerId, int? staffId, int? rentalId, int? storeId, int page, int pageSize)
    {
        var query = db.Payments.AsQueryable();

        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == customerId.Value);

        if (staffId.HasValue)
            query = query.Where(p => p.StaffId == staffId.Value);

        if (rentalId.HasValue)
            query = query.Where(p => p.RentalId == rentalId.Value);

        if (storeId.HasValue)
            query = query.Where(p => p.Rental.Inventory.StoreId == storeId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.PaymentId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentListResponse(
                p.PaymentId,
                p.RentalId,
                p.CustomerId,
                p.StaffId,
                p.Amount,
                p.PaymentDate))
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResponse<PaymentListResponse>(items, page, pageSize, totalCount, totalPages);
    }
}
