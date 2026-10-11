namespace Omranco.BLL.DTOS.Orders.Disputes;

public sealed record ApproveDisputeRefundResponse(
    int DisputeId,
    int RefundId,
    decimal ApprovedRefundAmount,
    OrderDisputeStatus Status
);