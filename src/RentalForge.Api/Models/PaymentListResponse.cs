namespace RentalForge.Api.Models;

public record PaymentListResponse(
    int Id,
    int RentalId,
    int CustomerId,
    int StaffId,
    decimal Amount,
    DateTime PaymentDate);
