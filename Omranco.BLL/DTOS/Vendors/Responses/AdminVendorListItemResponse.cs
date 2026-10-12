using Omranco.Domain.Enums.Vendors;

namespace Omranco.BLL.DTOS.Vendors.Responses;

public sealed record AdminVendorListItemResponse(
    int Id,
    int OwnerUserId,
    VendorType Type,
    string BusinessName,
    string GovernorateName,
    VendorStatus Status,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? SubmittedForReviewAt
);