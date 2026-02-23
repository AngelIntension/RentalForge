namespace RentalForge.Api.Models;

public record CreateRentalRequest
{
    public int FilmId { get; init; }
    public int StoreId { get; init; }
    public int CustomerId { get; init; }
    public int StaffId { get; init; }
}
