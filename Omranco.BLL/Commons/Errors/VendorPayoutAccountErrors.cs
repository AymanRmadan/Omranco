namespace Omranco.BLL.Commons.Errors;

public static class VendorPayoutAccountErrors
{
    public static readonly Error VendorNotFound =
        new(
            "VendorPayoutAccount.VendorNotFound",
            "Vendor was not found.",
            404
        );

    public static readonly Error NotFound =
        new(
            "VendorPayoutAccount.NotFound",
            "Payout account was not found.",
            404
        );

    public static readonly Error AccessDenied =
        new(
            "VendorPayoutAccount.AccessDenied",
            "You do not have access to this payout account.",
            400
        );

    public static readonly Error InvalidBankAccount =
        new(
            "VendorPayoutAccount.InvalidBankAccount",
            "Bank account information is incomplete.",
            400
        );

    public static readonly Error InvalidWallet =
        new(
            "VendorPayoutAccount.InvalidWallet",
            "Wallet information is incomplete.",
            400
        );

    public static readonly Error AlreadyVerified =
        new(
            "VendorPayoutAccount.AlreadyVerified",
            "This payout account is already verified.",
            400
        );
}