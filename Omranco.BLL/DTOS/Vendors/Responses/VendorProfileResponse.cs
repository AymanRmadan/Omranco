using Omranco.Domain.Enums.Vendors;

namespace Omranco.BLL.DTOS.Vendors.Responses;

public sealed record VendorProfileResponse(
    int Id,
    int OwnerUserId,
    VendorType Type,
    string BusinessName,
    int GovernorateId,
    string GovernorateName,
    string Address,
    decimal? Latitude,
    decimal? Longitude,
    VendorStatus Status,
    string? StatusReason,
    decimal RatingAverage,
    int RatingsCount,
    DateTime? SubmittedForReviewAt,
    DateTime? ApprovedAt,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<VendorCategoryResponse> Categories,
    List<VendorZoneResponse> ServiceZones,
    List<VendorDocumentResponse> Documents
);