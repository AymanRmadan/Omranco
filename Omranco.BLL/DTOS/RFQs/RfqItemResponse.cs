namespace Omranco.BLL.DTOS.Rfqs;

public sealed record RfqItemResponse(
    int Id,
    int ProductId,
    int? ProductVariantId,
    int UnitId,
    decimal Quantity,
    string? Notes
);