using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Enums.Payments;

namespace Omranco.BLL.DTOS.Orders.Responses;

public sealed record CustomerOrderDetailsResponse(
    int Id,
    string Number,
    int RfqId,
    int BidId,
    int ProjectId,
    string ProjectName,
    string? ProjectAddress,
    int VendorId,
    string VendorName,
    OrderStatus Status,
    PaymentStatus? PaymentStatus,
    decimal Subtotal,
    decimal ShippingCost,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? InProgressAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    List<CustomerOrderItemResponse> Items,
    List<OrderStatusHistoryResponse> StatusHistory
);