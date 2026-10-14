namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record UpdateProductAttributeRequest(
    string Name,
    bool IsRequired,
    int SortOrder,
    bool IsActive
);