namespace Omranco.BLL.DTOS.Rfqs;

public sealed record CreateRfqItemRequest(
    int ProductId,
    int? ProductVariantId,
    int UnitId,
    decimal Quantity,
    string? Notes
);