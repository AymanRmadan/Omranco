using Omranco.Domain.Enums.Orders;

namespace Omranco.BLL.DTOS.Orders.Responses;

public sealed record OrderStatusHistoryResponse(
    int Id,
    OrderStatus? OldStatus,
    OrderStatus NewStatus,
    int ChangedByUserId,
    DateTime ChangedAt
);