using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.Payouts;
using Omranco.Domain.Enums.Vendors;

namespace Omranco.BLL.Services.Abstractions.VendorServices;

public interface IVendorPayoutService
{
    // Vendor

    Task<Result<VendorWalletSummaryResponse>>
        GetWalletSummaryAsync(
            int currentUserId);

    Task<Result<List<VendorPayoutResponse>>>
        GetMyPayoutsAsync(
            int currentUserId);

    Task<Result<VendorPayoutResponse>>
        RequestAsync(
            int currentUserId,
            CreateVendorPayoutRequest request);

    Task<Result>
        CancelAsync(
            int payoutRequestId,
            int currentUserId);


    // Admin

    Task<Result<List<VendorPayoutResponse>>>
        GetAllAsync(
            VendorPayoutRequestStatus? status);

    Task<Result>
        ApproveAsync(
            int payoutRequestId,
            int currentUserId,
            ApproveVendorPayoutRequest request);

    Task<Result>
        RejectAsync(
            int payoutRequestId,
            int currentUserId,
            RejectVendorPayoutRequest request);

    Task<Result>
        StartProcessingAsync(
            int payoutRequestId,
            ProcessVendorPayoutRequest request);

    Task<Result>
        MarkPaidAsync(
            int payoutRequestId);

    Task<Result>
        MarkFailedAsync(
            int payoutRequestId,
            FailVendorPayoutRequest request);
}