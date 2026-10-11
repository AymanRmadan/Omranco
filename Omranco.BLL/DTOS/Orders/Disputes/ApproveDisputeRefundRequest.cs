namespace Omranco.BLL.DTOS.Orders.Disputes;

public sealed record ApproveDisputeRefundRequest(
    decimal Amount,
    string? ReviewNotes
);