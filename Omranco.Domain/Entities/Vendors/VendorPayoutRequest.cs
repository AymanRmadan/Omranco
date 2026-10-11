using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Enums.Vendors;

namespace Omranco.Domain.Entities.Vendors;

public sealed class VendorPayoutRequest : BaseEntity<int>
{
    public int VendorId { get; set; }

    public int VendorPayoutAccountId { get; set; }

    public decimal Amount { get; set; }

    public VendorPayoutRequestStatus Status { get; set; }
        = VendorPayoutRequestStatus.Pending;

    public string Reference { get; set; } = string.Empty;


    // ==========================================
    // Payout Account Snapshot
    // ==========================================

    public PayoutMethod PayoutMethod { get; set; }

    public string? AccountHolderName { get; set; }

    public string? BankName { get; set; }

    public string? AccountNumber { get; set; }

    public string? IBAN { get; set; }

    public string? WalletProvider { get; set; }

    public string? WalletPhoneNumber { get; set; }


    // ==========================================
    // Admin Review
    // ==========================================

    public int? ReviewedByUserId { get; set; }

    public string? ReviewNotes { get; set; }

    public DateTime? ReviewedAt { get; set; }


    // ==========================================
    // Processing
    // ==========================================

    public string? ExternalReference { get; set; }

    public string? FailureReason { get; set; }


    // ==========================================
    // Dates
    // ==========================================

    public DateTime RequestedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? ProcessingAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }


    // ==========================================
    // Navigation Properties
    // ==========================================

    public Vendor Vendor { get; set; } = null!;

    public VendorPayoutAccount VendorPayoutAccount { get; set; }
        = null!;

    public ApplicationUser? ReviewedByUser { get; set; }
}