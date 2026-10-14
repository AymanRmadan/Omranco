namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record UpdateProductRequest(
    int CategoryId,
    string Name,
    string? Description,
    string? ImagePath,
    bool IsActive
);