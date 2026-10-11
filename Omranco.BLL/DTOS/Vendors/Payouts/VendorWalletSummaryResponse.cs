namespace Omranco.BLL.DTOS.Vendors.Payouts;

public sealed record VendorWalletSummaryResponse(
    decimal Balance,
    decimal ReservedAmount,
    decimal AvailableToWithdraw,
    decimal TotalEarned,
    decimal TotalSettled
);