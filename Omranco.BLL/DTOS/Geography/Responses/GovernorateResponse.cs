namespace Omranco.BLL.DTOS.Geography.Responses;

public sealed record GovernorateResponse(
    int Id,
    string NameAr,
    string? NameEn,
    bool IsActive,
    List<ZoneResponse> Zones
);