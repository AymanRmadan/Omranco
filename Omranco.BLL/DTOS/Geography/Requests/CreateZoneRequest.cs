namespace Omranco.BLL.DTOS.Geography.Requests;

public sealed record CreateZoneRequest(
    int GovernorateId,
    string NameAr,
    string? NameEn
);
