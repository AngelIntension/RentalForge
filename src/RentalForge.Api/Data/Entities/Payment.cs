namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a payment for a rental. Links to customer, staff, and rental.
/// </summary>
public class Payment
{
    public int PaymentId { get; set; }
    public int CustomerId { get; set; }
    public int StaffId { get; set; }
    public int RentalId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }

    public Customer Customer { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
    public Rental Rental { get; set; } = null!;
}
