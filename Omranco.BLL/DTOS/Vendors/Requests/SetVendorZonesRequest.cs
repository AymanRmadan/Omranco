namespace Omranco.BLL.DTOS.Vendors.Requests;

public sealed record SetVendorZonesRequest(
    List<int> ZoneIds
);