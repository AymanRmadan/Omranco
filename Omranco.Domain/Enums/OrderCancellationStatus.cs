namespace Omranco.Domain.Enums.Orders;

public enum OrderCancellationStatus
{
    Requested = 1,
    RefundProcessing = 2,
    Completed = 3,
    Failed = 4
}