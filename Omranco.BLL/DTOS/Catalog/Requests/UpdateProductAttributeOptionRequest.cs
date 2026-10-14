namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record UpdateProductAttributeOptionRequest(
    string Value,
    int SortOrder,
    bool IsActive
);