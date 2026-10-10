namespace Omranco.Domain.Enums.Payments
{
    public enum PaymentStatus
    {
        Pending = 1,
        Paid = 2,
        PartiallyRefunded = 3,
        Refunded = 4,
        Cancelled = 5
    }
}