namespace Omranco.BLL.DTOS.Vendors.Responses;

public sealed record VendorZoneResponse(
    int ZoneId,
    string NameAr,
    string? NameEn
);