namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record CreateCategoryRequest(
    int? ParentId,
    string Name,
    string? Description,
    string? ImagePath,
    int SortOrder
);