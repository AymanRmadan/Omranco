namespace Omranco.Domain.Enums.Payments
{
    public enum PaymentAttemptStatus
    {
        Pending = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4,
        Cancelled = 5
    }
}