namespace Omranco.BLL.DTOS.Projects;

public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    int ZoneId,
    string? Address,
    decimal? Latitude,
    decimal? Longitude
);