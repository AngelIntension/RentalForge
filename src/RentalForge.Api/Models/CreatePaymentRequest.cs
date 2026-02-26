namespace RentalForge.Api.Models;

public record CreatePaymentRequest
{
    public int RentalId { get; init; }
    public decimal Amount { get; init; }
    public DateTime? PaymentDate { get; init; }
    public int StaffId { get; init; }
}
