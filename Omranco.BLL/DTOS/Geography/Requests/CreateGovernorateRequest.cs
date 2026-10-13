namespace Omranco.BLL.DTOS.Geography.Requests;

public sealed record CreateGovernorateRequest(
    string NameAr,
    string? NameEn
);