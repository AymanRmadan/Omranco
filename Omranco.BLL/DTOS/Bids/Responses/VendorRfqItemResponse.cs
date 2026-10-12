namespace Omranco.BLL.DTOS.Bids.Responses;

public sealed record VendorRfqItemResponse(
    int Id,
    int ProductId,
    int? ProductVariantId,
    int UnitId,
    decimal Quantity,
    string? Notes
);