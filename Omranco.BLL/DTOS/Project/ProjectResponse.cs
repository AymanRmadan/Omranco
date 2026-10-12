namespace Omranco.BLL.DTOS.Projects;

public sealed record ProjectResponse(
    int Id,
    int CustomerAccountId,
    int ZoneId,
    string Name,
    string? Description,
    string? Address,
    decimal? Latitude,
    decimal? Longitude,
    bool IsActive,
    DateTime CreatedAt
);