using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Entities.Payments;
using Omranco.Domain.Enums.Orders;

namespace Omranco.Domain.Entities.Orders;

public sealed class OrderDispute : BaseEntity<int>
{
    public int OrderId { get; set; }

    public int OpenedByAccountUserId { get; set; }

    public OrderDisputeType Type { get; set; }

    public OrderDisputeStatus Status { get; set; }
        = OrderDisputeStatus.Open;

    public string Reason { get; set; } = string.Empty;

    public string? Description { get; set; }


    // ==========================================
    // Admin Review
    // ==========================================

    public int? ReviewedByUserId { get; set; }

    public string? ReviewNotes { get; set; }

    /*
     * القيمة اللي Admin وافق يرجعها.
     *
     * null قبل المراجعة.
     */
    public decimal? ApprovedRefundAmount { get; set; }


    // ==========================================
    // Refund
    // ==========================================

    public int? PaymentRefundId { get; set; }


    // ==========================================
    // Timestamps
    // ==========================================

    public DateTime OpenedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }


    // ==========================================
    // Navigation Properties
    // ==========================================

    public Order Order { get; set; } = null!;

    public CustomerAccountUser OpenedByAccountUser { get; set; }
        = null!;

    public ApplicationUser? ReviewedByUser { get; set; }

    public PaymentRefund? PaymentRefund { get; set; }

    public ICollection<OrderDisputeAttachment> Attachments { get; set; }
        = new List<OrderDisputeAttachment>();
}