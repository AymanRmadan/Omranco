namespace Omranco.BLL.DTOS.Catalog.Responses;

public sealed record ProductSummaryResponse(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    string? ImagePath
);