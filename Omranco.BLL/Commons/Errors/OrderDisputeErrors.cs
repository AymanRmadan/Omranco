namespace Omranco.BLL.Commons.Errors;

public static class OrderDisputeErrors
{
    public static readonly Error NotFound =
        new(
            "OrderDispute.NotFound",
            "Order dispute was not found.",
            404
        );

    public static readonly Error AccessDenied =
        new(
            "OrderDispute.AccessDenied",
            "You do not have access to this order.",
            400
        );

    public static readonly Error InvalidOrderStatus =
        new(
            "OrderDispute.InvalidOrderStatus",
            "A dispute can only be opened for a shipped or delivered order.",
            400
        );

    public static readonly Error ActiveDisputeExists =
        new(
            "OrderDispute.ActiveDisputeExists",
            "An active dispute already exists for this order.",
            400
        );

    public static readonly Error ReasonRequired =
        new(
            "OrderDispute.ReasonRequired",
            "A dispute reason is required.",
            400
        );

    public static readonly Error InvalidStatus =
    new(
        "OrderDispute.InvalidStatus",
        "The dispute cannot be updated in its current status.",
        400
    );

    public static readonly Error ReviewNotesRequired =
        new(
            "OrderDispute.ReviewNotesRequired",
            "Review notes are required when rejecting a dispute.",
            400
        );

    public static readonly Error InvalidRefundAmount =
    new(
        "OrderDispute.InvalidRefundAmount",
        "Approved refund amount must be greater than zero.",
        400
    );

    public static readonly Error RefundAmountExceedsAvailable =
        new(
            "OrderDispute.RefundAmountExceedsAvailable",
            "Approved refund amount exceeds the remaining refundable amount.",
            400
        );
}