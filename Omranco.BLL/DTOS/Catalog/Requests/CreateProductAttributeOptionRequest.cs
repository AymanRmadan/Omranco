namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record CreateProductAttributeOptionRequest(
    string Value,
    int SortOrder
);