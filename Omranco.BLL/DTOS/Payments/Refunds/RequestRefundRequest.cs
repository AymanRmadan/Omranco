namespace Omranco.BLL.DTOS.Payments.Refunds;

public sealed record RequestRefundRequest(
    decimal Amount,
    string? Reason
);