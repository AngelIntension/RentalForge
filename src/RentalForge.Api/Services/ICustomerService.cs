using Ardalis.Result;
using RentalForge.Api.Models;

namespace RentalForge.Api.Services;

/// <summary>
/// Defines operations for managing customer entities.
/// </summary>
public interface ICustomerService
{
    Task<PagedResponse<CustomerResponse>> GetCustomersAsync(string? search, int page, int pageSize);
    Task<Result<CustomerResponse>> GetCustomerByIdAsync(int id);
    Task<Result<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request);
    Task<Result<CustomerResponse>> UpdateCustomerAsync(int id, UpdateCustomerRequest request);
    Task<Result> DeactivateCustomerAsync(int id);
}
