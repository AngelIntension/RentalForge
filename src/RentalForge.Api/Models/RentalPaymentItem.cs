namespace RentalForge.Api.Models;

public record RentalPaymentItem(
    int Id,
    decimal Amount,
    DateTime PaymentDate,
    int StaffId);
