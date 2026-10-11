using Omranco.Domain.Enums.Orders;

namespace Omranco.BLL.DTOS.Orders.Disputes;

public sealed record OpenOrderDisputeRequest(
    OrderDisputeType Type,
    string Reason,
    string? Description
);