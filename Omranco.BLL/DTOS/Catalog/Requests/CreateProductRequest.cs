namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record CreateProductRequest(
    int CategoryId,
    string Name,
    string? Description,
    string? ImagePath,
    List<ProductUnitRequest> Units
);