using Ardalis.Result;
using RentalForge.Api.Models;

namespace RentalForge.Api.Services;

public interface IPaymentService
{
    Task<Result<PaymentDetailResponse>> CreatePaymentAsync(CreatePaymentRequest request);
    Task<PagedResponse<PaymentListResponse>> GetPaymentsAsync(
        int? customerId, int? staffId, int? rentalId, int? storeId, int page, int pageSize);
}
