namespace Omranco.BLL.DTOS.Catalog.Responses;

public sealed record CategoryTreeResponse(
    int Id,
    int? ParentId,
    string Name,
    string? Description,
    string? ImagePath,
    int SortOrder,
    List<CategoryTreeResponse> Children
);