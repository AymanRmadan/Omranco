namespace Omranco.BLL.Commons.Errors;

public static class PaymobErrors
{
    public static readonly Error InvalidAmount =
        new(
            "Paymob.InvalidAmount",
            "Payment amount must be greater than zero.",
            400
        );

    public static readonly Error UnsupportedPaymentMethod =
        new(
            "Paymob.UnsupportedPaymentMethod",
            "The selected payment method is not configured.",
            400
        );

    public static readonly Error CreateIntentionFailed =
        new(
            "Paymob.CreateIntentionFailed",
            "Failed to initialize payment with Paymob.",
            400
        );

    public static readonly Error InvalidResponse =
        new(
            "Paymob.InvalidResponse",
            "Invalid response received from Paymob.",
            400
        );

    public static readonly Error InvalidTransactionId =
    new(
        "Paymob.InvalidTransactionId",
        "Invalid Paymob transaction id.",
        400
    );

    public static readonly Error RefundFailed =
        new(
            "Paymob.RefundFailed",
            "Paymob rejected the refund request.",
            400
        );

    public static readonly Error RefundStatusUnknown =
        new(
            "Paymob.RefundStatusUnknown",
            "The refund request status could not be confirmed.",
            400
        );
}