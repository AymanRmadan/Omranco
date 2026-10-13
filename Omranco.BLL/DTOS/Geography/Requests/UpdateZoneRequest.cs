namespace Omranco.BLL.DTOS.Geography.Requests;

public sealed record UpdateZoneRequest(
    int GovernorateId,
    string NameAr,
    string? NameEn,
    bool IsActive
);