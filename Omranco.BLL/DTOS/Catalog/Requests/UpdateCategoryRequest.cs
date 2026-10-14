namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record UpdateCategoryRequest(
    int? ParentId,
    string Name,
    string? Description,
    string? ImagePath,
    int SortOrder,
    bool IsActive
);