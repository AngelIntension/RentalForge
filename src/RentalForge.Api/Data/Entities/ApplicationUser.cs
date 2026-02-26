using Microsoft.AspNetCore.Identity;

namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Application user extending ASP.NET Core Identity. Optionally linked to a dvdrental Customer.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public int? CustomerId { get; set; }
    public int? StaffId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Customer? Customer { get; set; }
    public Staff? Staff { get; set; }
}
