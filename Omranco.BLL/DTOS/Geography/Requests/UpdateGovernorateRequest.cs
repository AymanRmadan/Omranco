namespace Omranco.BLL.DTOS.Geography.Requests;

public sealed record UpdateGovernorateRequest(
    string NameAr,
    string? NameEn,
    bool IsActive
);