using Omranco.Domain.Enums.Payments;

namespace Omranco.Domain.Entities.Payments
{
    public sealed class PaymentAttempt : BaseEntity<int>
    {
        public int PaymentId { get; set; }

        public PaymentMethod Method { get; set; }

        public PaymentAttemptStatus Status { get; set; }
            = PaymentAttemptStatus.Pending;

        public decimal Amount { get; set; }

        // Reference من Omranco
        public string Reference { get; set; } = string.Empty;


        // ==========================================
        // Paymob References
        // ==========================================

        public string? PaymobIntentionId { get; set; }

        public string? PaymobOrderId { get; set; }

        public string? PaymobTransactionId { get; set; }


        // ==========================================
        // Failure Information
        // ==========================================

        public string? FailureCode { get; set; }

        public string? FailureMessage { get; set; }


        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }


        // Navigation Properties

        public Payment Payment { get; set; } = null!;
    }
}