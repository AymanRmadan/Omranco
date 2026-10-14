namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record CreateProductVariantRequest(
    string? Name,
    string? Sku,
    List<ProductVariantValueRequest> Values
);