using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Enums.Vendors;

public sealed class VendorDocument : BaseEntity<int>
{

    public int VendorId { get; set; }

    public int RequirementId { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public DocumentReviewStatus ReviewStatus { get; set; }
        = DocumentReviewStatus.Pending;

    public string? ReviewNotes { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }


    public Vendor Vendor { get; set; } = null!;

    public VendorDocumentRequirement Requirement { get; set; } = null!;

    public ApplicationUser? ReviewedByUser { get; set; }
}