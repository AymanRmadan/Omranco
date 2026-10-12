using Omranco.Domain.Enums.Vendors;

namespace Omranco.BLL.DTOS.Vendors.Responses;

public sealed record VendorDocumentResponse(
    int Id,
    int RequirementId,
    string RequirementName,
    bool IsRequired,
    string FilePath,
    DocumentReviewStatus ReviewStatus,
    string? ReviewNotes,
    DateTime UploadedAt,
    DateTime? ReviewedAt
);