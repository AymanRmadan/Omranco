using Omranco.BLL;

public static class OrderCancellationErrors
{
    public static readonly Error NotAllowed =
        new(
            "OrderCancellation.NotAllowed",
            "The order can no longer be cancelled.",
            400
        );

    public static readonly Error AlreadyRequested =
        new(
            "OrderCancellation.AlreadyRequested",
            "A cancellation request already exists for this order.",
            400
        );

    public static readonly Error PaymentNotRefundable =
        new(
            "OrderCancellation.PaymentNotRefundable",
            "The order payment cannot be refunded.",
            400
        );
}