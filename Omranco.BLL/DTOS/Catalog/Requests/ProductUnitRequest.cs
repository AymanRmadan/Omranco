namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record ProductUnitRequest(
    int UnitId,
    bool IsDefault
);