namespace Omranco.BLL.Commons.Errors;

public static class VendorPayoutErrors
{
    public static readonly Error VendorNotFound =
        new(
            "VendorPayout.VendorNotFound",
            "Vendor was not found.",
            404
        );

    public static readonly Error WalletNotFound =
        new(
            "VendorPayout.WalletNotFound",
            "Vendor wallet was not found.",
            404
        );

    public static readonly Error NotFound =
        new(
            "VendorPayout.NotFound",
            "Payout request was not found.",
            404
        );

    public static readonly Error InvalidAmount =
        new(
            "VendorPayout.InvalidAmount",
            "Payout amount must be greater than zero.",
            400
        );

    public static readonly Error InsufficientAvailableBalance =
        new(
            "VendorPayout.InsufficientAvailableBalance",
            "The available wallet balance is insufficient for this payout.",
            400
        );

    public static readonly Error PayoutAccountNotFound =
        new(
            "VendorPayout.PayoutAccountNotFound",
            "Payout account was not found.",
            404
        );

    public static readonly Error PayoutAccountNotVerified =
        new(
            "VendorPayout.PayoutAccountNotVerified",
            "The payout account must be verified before it can be used.",
            400
        );

    public static readonly Error InvalidPayoutAccount =
        new(
            "VendorPayout.InvalidPayoutAccount",
            "The payout account information is incomplete.",
            400
        );

    public static readonly Error InvalidStatus =
        new(
            "VendorPayout.InvalidStatus",
            "The payout request cannot be updated in its current status.",
            400
        );

    public static readonly Error AccessDenied =
        new(
            "VendorPayout.AccessDenied",
            "You do not have access to this payout request.",
            400
        );

    public static readonly Error ReviewNotesRequired =
        new(
            "VendorPayout.ReviewNotesRequired",
            "Review notes are required when rejecting a payout request.",
            400
        );

    public static readonly Error FailureReasonRequired =
        new(
            "VendorPayout.FailureReasonRequired",
            "A failure reason is required.",
            400
        );
}