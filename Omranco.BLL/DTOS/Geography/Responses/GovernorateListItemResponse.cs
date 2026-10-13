namespace Omranco.BLL.DTOS.Geography.Responses;

public sealed record GovernorateListItemResponse(
    int Id,
    string NameAr,
    string? NameEn,
    bool IsActive
);