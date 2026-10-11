using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Enums.Payments;

namespace Omranco.Domain.Entities.Payments;

public sealed class PaymentRefund : BaseEntity<int>
{
    public int PaymentId { get; set; }

    /*
     * محاولة الدفع الناجحة الأصلية
     * اللي هنرجع منها الفلوس.
     */
    public int PaymentAttemptId { get; set; }

    public decimal Amount { get; set; }

    public PaymentRefundStatus Status { get; set; }
        = PaymentRefundStatus.Pending;

    /*
     * Reference داخلي من Omranco.
     */
    public string Reference { get; set; } = string.Empty;

    public string? Reason { get; set; }

    /*
     * Transaction ID الخاص بعملية Refund
     * اللي هيرجعه Paymob بعدين.
     */
    public string? PaymobRefundTransactionId { get; set; }

    public string? FailureCode { get; set; }

    public string? FailureMessage { get; set; }


    /*
     * المستخدم اللي طلب الـRefund.
     *
     * nullable عشان مستقبلًا ممكن يكون
     * System/Admin operation.
     */
    public int? RequestedByUserId { get; set; }


    public DateTime RequestedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }


    // Navigation Properties

    public Payment Payment { get; set; } = null!;

    public PaymentAttempt PaymentAttempt { get; set; } = null!;

    public ApplicationUser? RequestedByUser { get; set; }
}