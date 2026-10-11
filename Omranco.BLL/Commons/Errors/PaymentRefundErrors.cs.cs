namespace Omranco.BLL.Commons.Errors;

public static class PaymentRefundErrors
{
    public static readonly Error InvalidAmount =
        new(
            "PaymentRefund.InvalidAmount",
            "Refund amount must be greater than zero.",
            400
        );

    public static readonly Error PaymentNotRefundable =
        new(
            "PaymentRefund.PaymentNotRefundable",
            "This payment cannot be refunded in its current status.",
            400
        );

    public static readonly Error AmountExceedsRefundable =
        new(
            "PaymentRefund.AmountExceedsRefundable",
            "Refund amount exceeds the remaining refundable amount.",
            400
        );

    public static readonly Error SuccessfulAttemptNotFound =
        new(
            "PaymentRefund.SuccessfulAttemptNotFound",
            "The successful payment transaction could not be found.",
            404
        );

    public static readonly Error ActiveRefundExists =
        new(
            "PaymentRefund.ActiveRefundExists",
            "Another refund is already being processed for this payment.",
            400
        );

    public static readonly Error InvalidProviderTransaction =
    new(
        "PaymentRefund.InvalidProviderTransaction",
        "The original Paymob transaction id is invalid.",
        400
    );

    public static readonly Error RefundNotFound =
    new(
        "PaymentRefund.NotFound",
        "Refund transaction was not found.",
        404
    );
}