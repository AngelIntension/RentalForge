namespace RentalForge.Api.Models;

public record PaymentDetailResponse(
    int Id,
    int RentalId,
    int CustomerId,
    string CustomerFirstName,
    string CustomerLastName,
    int StaffId,
    string StaffFirstName,
    string StaffLastName,
    decimal Amount,
    DateTime PaymentDate,
    string FilmTitle);
