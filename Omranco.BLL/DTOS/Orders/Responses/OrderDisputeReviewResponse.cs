using Omranco.Domain.Enums.Orders;

namespace Omranco.BLL.DTOS.Orders.Disputes;

public sealed record OrderDisputeReviewResponse(
    int DisputeId,
    OrderDisputeStatus Status,
    int? ReviewedByUserId,
    DateTime? ReviewedAt
);