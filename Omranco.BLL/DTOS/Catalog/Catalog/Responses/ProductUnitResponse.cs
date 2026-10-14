namespace Omranco.BLL.DTOS.Catalog.Responses;

public sealed record ProductUnitResponse(
    int UnitId,
    string Name,
    string? Symbol,
    bool IsDefault
);