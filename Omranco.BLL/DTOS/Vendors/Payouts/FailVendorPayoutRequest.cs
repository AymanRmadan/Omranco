namespace Omranco.BLL.DTOS.Vendors.Payouts;

public sealed record FailVendorPayoutRequest(
    string FailureReason
);