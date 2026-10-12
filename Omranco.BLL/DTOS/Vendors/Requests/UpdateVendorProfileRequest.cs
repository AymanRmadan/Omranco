namespace Omranco.BLL.DTOS.Vendors.Requests;

public sealed record UpdateVendorProfileRequest(
    string BusinessName,
    int GovernorateId,
    string Address,
    decimal? Latitude,
    decimal? Longitude
);