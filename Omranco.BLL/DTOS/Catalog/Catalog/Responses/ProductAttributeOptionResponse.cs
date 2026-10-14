namespace Omranco.BLL.DTOS.Catalog.Responses;

public sealed record ProductAttributeOptionResponse(
    int Id,
    string Value,
    int SortOrder
);