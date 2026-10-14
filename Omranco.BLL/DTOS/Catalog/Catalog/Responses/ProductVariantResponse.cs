namespace Omranco.BLL.DTOS.Catalog.Responses;

public sealed record ProductVariantResponse(
    int Id,
    string? Name,
    string? Sku,
    List<ProductVariantValueResponse> Values
);