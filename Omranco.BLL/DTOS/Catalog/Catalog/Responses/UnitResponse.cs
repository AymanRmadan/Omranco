namespace Omranco.BLL.DTOS.Catalog.Responses;

public sealed record UnitResponse(
    int Id,
    string Name,
    string? Symbol
);