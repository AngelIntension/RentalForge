namespace RentalForge.Api.Models;

public record RentalDetailResponse(
    int Id,
    DateTime RentalDate,
    DateTime? ReturnDate,
    int InventoryId,
    int FilmId,
    string FilmTitle,
    int StoreId,
    int CustomerId,
    string CustomerFirstName,
    string CustomerLastName,
    int StaffId,
    string StaffFirstName,
    string StaffLastName,
    DateTime LastUpdate);
