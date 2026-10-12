using Omranco.Domain.Enums.Vendors;

namespace Omranco.BLL.DTOS.Vendors.Responses;

public sealed record VendorDocumentRequirementResponse(
    int Id,
    string Name,
    VendorType? VendorType,
    bool IsRequired,
    int SortOrder,
    VendorDocumentResponse? Document
);
