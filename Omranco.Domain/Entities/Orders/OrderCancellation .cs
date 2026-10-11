using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Entities.Payments;
using Omranco.Domain.Enums.Orders;

namespace Omranco.Domain.Entities.Orders;

public sealed class OrderCancellation : BaseEntity<int>
{
    public int OrderId { get; set; }

    public int RequestedByUserId { get; set; }

    public string? Reason { get; set; }

    public OrderCancellationStatus Status { get; set; }
        = OrderCancellationStatus.Requested;

    public int? PaymentRefundId { get; set; }

    public string? FailureReason { get; set; }

    public DateTime RequestedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }


    public Order Order { get; set; } = null!;

    public ApplicationUser RequestedByUser { get; set; } = null!;

    public PaymentRefund? PaymentRefund { get; set; }
}