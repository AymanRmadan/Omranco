namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record CreateUnitRequest(
    string Name,
    string? Symbol
);