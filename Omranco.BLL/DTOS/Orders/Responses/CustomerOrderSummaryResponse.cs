using Omranco.Domain.Enums.Orders;

namespace Omranco.BLL.DTOS.Orders.Responses;

public sealed record CustomerOrderSummaryResponse(
    int Id,
    string Number,
    int ProjectId,
    string ProjectName,
    int VendorId,
    string VendorName,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? DeliveredAt
);