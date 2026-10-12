namespace Omranco.BLL.DTOS.Vendors.Requests;

public sealed record SetVendorCategoriesRequest(
    List<int> CategoryIds
);