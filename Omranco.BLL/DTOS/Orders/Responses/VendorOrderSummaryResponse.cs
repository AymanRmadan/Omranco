using Omranco.Domain.Enums.Orders;

namespace Omranco.BLL.DTOS.Orders.Responses;

public sealed record VendorOrderSummaryResponse(
    int Id,
    string Number,
    int ProjectId,
    string ProjectName,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? DeliveredAt
);