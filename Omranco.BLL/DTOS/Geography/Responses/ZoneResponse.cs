namespace Omranco.BLL.DTOS.Geography.Responses;

public sealed record ZoneResponse(
    int Id,
    int GovernorateId,
    string NameAr,
    string? NameEn,
    bool IsActive
);