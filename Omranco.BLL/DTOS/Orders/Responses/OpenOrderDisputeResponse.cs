using Omranco.Domain.Enums.Orders;

namespace Omranco.BLL.DTOS.Orders.Disputes;

public sealed record OpenOrderDisputeResponse(
    int DisputeId,
    OrderDisputeType Type,
    OrderDisputeStatus Status,
    DateTime OpenedAt
);