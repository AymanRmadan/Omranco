namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record UpdateUnitRequest(
    string Name,
    string? Symbol,
    bool IsActive
);