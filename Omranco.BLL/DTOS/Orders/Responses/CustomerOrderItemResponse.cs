namespace Omranco.BLL.DTOS.Orders.Responses;

public sealed record CustomerOrderItemResponse(
    int ProductId,
    string ProductName,
    int? ProductVariantId,
    string? ProductVariantName,
    int UnitId,
    string UnitName,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);