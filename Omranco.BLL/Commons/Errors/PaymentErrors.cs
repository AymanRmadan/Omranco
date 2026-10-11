namespace Omranco.BLL.Commons.Errors;

public static class PaymentErrors
{
    public static readonly Error NotFound =
        new(
            "Payment.NotFound",
            "Payment not found.",
            404
        );

    public static readonly Error AccessDenied =
        new(
            "Payment.AccessDenied",
            "You do not have access to this payment.",
            401
        );

    public static readonly Error AlreadyPaid =
        new(
            "Payment.AlreadyPaid",
            "This payment has already been completed.",
            400
        );

    public static readonly Error InvalidStatus =
        new(
            "Payment.InvalidStatus",
            "This payment cannot be processed in its current status.",
            400
        );

    public static readonly Error ActiveAttemptExists =
        new(
            "Payment.ActiveAttemptExists",
            "An active payment attempt already exists.",
            400
        );

    public static readonly Error InitializationFailed =
        new(
            "Payment.InitializationFailed",
            "Payment could not be initialized.",
            400
        );

    public static readonly Error InvalidWebhookSignature =
    new(
        "Payment.InvalidWebhookSignature",
        "Invalid Paymob webhook signature.",
        400
    );


    public static readonly Error AttemptNotFound =
        new(
            "Payment.AttemptNotFound",
            "Payment attempt was not found.",
            404
        );


    public static readonly Error AmountMismatch =
        new(
            "Payment.AmountMismatch",
            "Payment amount does not match the expected amount.",
            400
        );


    public static readonly Error CurrencyMismatch =
        new(
            "Payment.CurrencyMismatch",
            "Payment currency does not match the expected currency.",
            400
        );
}