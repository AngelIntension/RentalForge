namespace RentalForge.Api.Models;

public record RentalListResponse(
    int Id,
    DateTime RentalDate,
    DateTime? ReturnDate,
    int InventoryId,
    int CustomerId,
    int StaffId,
    DateTime LastUpdate,
    decimal TotalPaid = 0,
    decimal RentalRate = 0,
    decimal OutstandingBalance = 0);
