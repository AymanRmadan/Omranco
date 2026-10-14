namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record ProductVariantValueRequest(
    int ProductAttributeId,
    int ProductAttributeOptionId
);