using Omranco.Domain.Enums.Vendors;

namespace Omranco.BLL.DTOS.Vendors.Requests;

public sealed record CreateVendorProfileRequest(
    VendorType Type,
    string BusinessName,
    int GovernorateId,
    string Address,
    decimal? Latitude,
    decimal? Longitude
);