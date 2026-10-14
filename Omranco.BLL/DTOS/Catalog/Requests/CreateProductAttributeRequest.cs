namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record CreateProductAttributeRequest(
    string Name,
    bool IsRequired,
    int SortOrder
);