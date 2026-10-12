namespace Omranco.BLL.DTOS.Orders.Responses;

public sealed record VendorOrderDetailsResponse(
    int Id,
    string Number,
    string Status,
    string ProjectName,
    string? ProjectAddress,
    string CustomerName,
    string? CustomerPhoneNumber,
    decimal Subtotal,
    decimal ShippingCost,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? DeliveredAt,
    List<VendorOrderItemResponse> Items
);