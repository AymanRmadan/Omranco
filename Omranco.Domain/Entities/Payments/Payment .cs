using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Enums.Payments;

namespace Omranco.Domain.Entities.Payments
{
    public sealed class Payment : BaseEntity<int>
    {
        public int OrderId { get; set; }

        // المبلغ المطلوب من العميل
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        public PaymentStatus Status { get; set; }
            = PaymentStatus.Pending;

        // المبلغ اللي تم تحصيله فعليًا
        public decimal PaidAmount { get; set; }

        // إجمالي المبلغ اللي اتعمله Refund
        public decimal RefundedAmount { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties

        public Order Order { get; set; } = null!;

        public ICollection<PaymentAttempt> Attempts { get; set; }
            = new List<PaymentAttempt>();

        public ICollection<PaymentRefund> Refunds { get; set; } = new List<PaymentRefund>();
    }
}