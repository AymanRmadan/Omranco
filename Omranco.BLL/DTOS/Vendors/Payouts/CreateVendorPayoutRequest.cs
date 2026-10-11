namespace Omranco.BLL.DTOS.Vendors.Payouts;

public sealed record CreateVendorPayoutRequest(
    int VendorPayoutAccountId,
    decimal Amount
);