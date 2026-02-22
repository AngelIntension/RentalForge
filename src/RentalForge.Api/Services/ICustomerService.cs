using RentalForge.Api.Models;

namespace RentalForge.Api.Services;

/// <summary>
/// Defines operations for managing customer entities.
/// </summary>
public interface ICustomerService
{
    Task<PagedResponse<CustomerResponse>> GetCustomersAsync(string? search, int page, int pageSize);
    Task<CustomerResponse?> GetCustomerByIdAsync(int id);
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerResponse?> UpdateCustomerAsync(int id, UpdateCustomerRequest request);
    Task<bool> DeactivateCustomerAsync(int id);
}
