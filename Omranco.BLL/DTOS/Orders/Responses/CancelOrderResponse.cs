using Omranco.Domain.Enums.Orders;

namespace Omranco.BLL.DTOS.Orders.Cancellations;

public sealed record CancelOrderResponse(
    int CancellationId,
    OrderCancellationStatus Status,
    int? RefundId
);