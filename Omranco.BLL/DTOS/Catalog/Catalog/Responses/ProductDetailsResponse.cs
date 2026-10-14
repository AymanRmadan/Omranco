namespace Omranco.BLL.DTOS.Catalog.Responses;

public sealed record ProductDetailsResponse(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    string? ImagePath,
    List<ProductUnitResponse> Units,
    List<ProductAttributeResponse> Attributes,
    List<ProductVariantResponse> Variants
);