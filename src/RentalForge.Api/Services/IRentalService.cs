using Ardalis.Result;
using RentalForge.Api.Models;

namespace RentalForge.Api.Services;

public interface IRentalService
{
    Task<PagedResponse<RentalListResponse>> GetRentalsAsync(int? customerId, bool activeOnly, int page, int pageSize);
    Task<Result<RentalDetailResponse>> GetRentalByIdAsync(int id);
    Task<Result<RentalDetailResponse>> CreateRentalAsync(CreateRentalRequest request);
    Task<Result<RentalDetailResponse>> ReturnRentalAsync(int id);
    Task<Result> DeleteRentalAsync(int id);
}
