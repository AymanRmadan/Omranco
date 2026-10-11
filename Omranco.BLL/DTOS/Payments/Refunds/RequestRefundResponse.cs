namespace Omranco.BLL.DTOS.Payments.Refunds;

public sealed record RequestRefundResponse(
    int RefundId,
    decimal Amount,
    decimal RemainingRefundableAmount
);