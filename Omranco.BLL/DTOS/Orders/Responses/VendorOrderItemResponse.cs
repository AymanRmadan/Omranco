namespace Omranco.BLL.DTOS.Orders.Responses
{
    public record VendorOrderItemResponse(
        int ProductId,
        string ProductName,
        int? ProductVariantId,
        string? VariantName,
        int UnitId,
        string UnitName,
        decimal Quantity,
        decimal UnitPrice,
        decimal TotalPrice);

}