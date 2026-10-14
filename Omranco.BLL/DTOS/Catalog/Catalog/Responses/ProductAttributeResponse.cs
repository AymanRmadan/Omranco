namespace Omranco.BLL.DTOS.Catalog.Responses;

public sealed record ProductAttributeResponse(
    int Id,
    string Name,
    bool IsRequired,
    int SortOrder,
    List<ProductAttributeOptionResponse> Options
);