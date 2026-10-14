namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record UpdateProductVariantRequest(
    string? Name,
    string? Sku,
    bool IsActive,
    List<ProductVariantValueRequest> Values
);