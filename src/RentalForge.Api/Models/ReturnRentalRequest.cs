namespace RentalForge.Api.Models;

public record ReturnRentalRequest
{
    public decimal? Amount { get; init; }
    public int? StaffId { get; init; }
}
