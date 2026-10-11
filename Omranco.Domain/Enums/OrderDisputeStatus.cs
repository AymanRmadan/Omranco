namespace Omranco.Domain.Enums.Orders;

public enum OrderDisputeStatus
{
    Open = 1,
    UnderReview = 2,
    RefundProcessing = 3,
    Resolved = 4,
    Rejected = 5,
    Cancelled = 6
}