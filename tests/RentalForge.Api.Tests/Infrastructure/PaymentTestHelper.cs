using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Tests.Infrastructure;

/// <summary>
/// Seeds test data for payment integration tests.
/// Uses high ID range (9000+) to avoid collisions with reference data.
/// Call after RentalTestHelper.SeedTestDataAsync() which provides stores, staff, customers, films, inventory, and rentals.
/// </summary>
public static class PaymentTestHelper
{
    /// <summary>
    /// Seeds a single payment for rental 9001 (active rental, customer 9001, staff 9001).
    /// </summary>
    public static async Task SeedPaymentAsync(DvdrentalContext db)
    {
        var payment = new Payment
        {
            PaymentId = 9010, CustomerId = 9001, StaffId = 9001,
            RentalId = 9001, Amount = 3.99m, PaymentDate = DateTime.UtcNow.AddDays(-4)
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds multiple payments across different rentals, customers, and staff
    /// for testing list/filter/pagination scenarios.
    /// </summary>
    public static async Task SeedMultiplePaymentsAsync(DvdrentalContext db)
    {
        var payments = new List<Payment>
        {
            // Payment for rental 9001 (store 9001, customer 9001)
            new()
            {
                PaymentId = 9011, CustomerId = 9001, StaffId = 9001,
                RentalId = 9001, Amount = 3.99m, PaymentDate = DateTime.UtcNow.AddDays(-4)
            },
            // Second payment for rental 9001 (same rental, different time)
            new()
            {
                PaymentId = 9012, CustomerId = 9001, StaffId = 9001,
                RentalId = 9001, Amount = 1.00m, PaymentDate = DateTime.UtcNow.AddDays(-3)
            },
            // Payment for rental 9002 (store 9001, customer 9002, returned)
            new()
            {
                PaymentId = 9013, CustomerId = 9002, StaffId = 9001,
                RentalId = 9002, Amount = 3.99m, PaymentDate = DateTime.UtcNow.AddDays(-3)
            },
            // Payment for rental 9003 (store 9001, customer 9001)
            new()
            {
                PaymentId = 9014, CustomerId = 9001, StaffId = 9001,
                RentalId = 9003, Amount = 2.99m, PaymentDate = DateTime.UtcNow.AddDays(-1)
            },
        };
        db.Payments.AddRange(payments);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a payment for a specific rental. Useful for targeted test scenarios.
    /// </summary>
    public static async Task SeedPaymentForRentalAsync(
        DvdrentalContext db, int paymentId, int rentalId, int customerId, int staffId, decimal amount)
    {
        var payment = new Payment
        {
            PaymentId = paymentId, CustomerId = customerId, StaffId = staffId,
            RentalId = rentalId, Amount = amount, PaymentDate = DateTime.UtcNow
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
    }
}
