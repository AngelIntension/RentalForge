using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Models;

namespace RentalForge.Api.Services;

/// <summary>
/// Implements customer CRUD operations against the dvdrental database.
/// </summary>
public class CustomerService(DvdrentalContext db, ILogger<CustomerService> logger) : ICustomerService
{
    public async Task<PagedResponse<CustomerResponse>> GetCustomersAsync(string? search, int page, int pageSize)
    {
        var query = db.Customers.Where(c => c.Activebool);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.FirstName, pattern) ||
                EF.Functions.ILike(c.LastName, pattern) ||
                (c.Email != null && EF.Functions.ILike(c.Email, pattern)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => ToResponse(c))
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResponse<CustomerResponse>(items, page, pageSize, totalCount, totalPages);
    }

    public async Task<CustomerResponse?> GetCustomerByIdAsync(int id)
    {
        var customer = await db.Customers
            .Where(c => c.CustomerId == id && c.Activebool)
            .Select(c => ToResponse(c))
            .FirstOrDefaultAsync();

        return customer;
    }

    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (!await db.Stores.AnyAsync(s => s.StoreId == request.StoreId))
            errors["storeId"] = [$"Store with ID {request.StoreId} does not exist."];

        if (!await db.Addresses.AnyAsync(a => a.AddressId == request.AddressId))
            errors["addressId"] = [$"Address with ID {request.AddressId} does not exist."];

        if (errors.Count > 0)
            throw new ServiceValidationException(errors);

        var customer = new Customer
        {
            StoreId = request.StoreId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            AddressId = request.AddressId,
            Activebool = true,
            Active = 1,
            CreateDate = DateOnly.FromDateTime(DateTime.UtcNow),
            LastUpdate = DateTime.UtcNow
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        logger.LogInformation("Created customer {CustomerId} ({FirstName} {LastName})",
            customer.CustomerId, customer.FirstName, customer.LastName);

        return ToResponse(customer);
    }

    public async Task<CustomerResponse?> UpdateCustomerAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == id && c.Activebool);

        if (customer is null)
            return null;

        var errors = new Dictionary<string, string[]>();

        if (!await db.Stores.AnyAsync(s => s.StoreId == request.StoreId))
            errors["storeId"] = [$"Store with ID {request.StoreId} does not exist."];

        if (!await db.Addresses.AnyAsync(a => a.AddressId == request.AddressId))
            errors["addressId"] = [$"Address with ID {request.AddressId} does not exist."];

        if (errors.Count > 0)
            throw new ServiceValidationException(errors);

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.StoreId = request.StoreId;
        customer.AddressId = request.AddressId;
        customer.LastUpdate = DateTime.UtcNow;

        await db.SaveChangesAsync();

        logger.LogInformation("Updated customer {CustomerId} ({FirstName} {LastName})",
            customer.CustomerId, customer.FirstName, customer.LastName);

        return ToResponse(customer);
    }

    public async Task<bool> DeactivateCustomerAsync(int id)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == id && c.Activebool);

        if (customer is null)
            return false;

        customer.Activebool = false;
        customer.Active = 0;
        customer.LastUpdate = DateTime.UtcNow;

        await db.SaveChangesAsync();

        logger.LogInformation("Deactivated customer {CustomerId}", customer.CustomerId);

        return true;
    }

    private static CustomerResponse ToResponse(Customer c) => new(
        Id: c.CustomerId,
        StoreId: c.StoreId,
        FirstName: c.FirstName,
        LastName: c.LastName,
        Email: c.Email,
        AddressId: c.AddressId,
        IsActive: c.Activebool,
        CreateDate: c.CreateDate,
        LastUpdate: c.LastUpdate);
}
