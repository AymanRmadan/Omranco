using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Entities.Geography;
using Omranco.Domain.Enums.Vendors;

namespace Omranco.Domain.Entities.Vendors
{
    public sealed class Vendor
    {
        public int Id { get; set; }

        public int OwnerUserId { get; set; }

        public VendorType Type { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public int GovernorateId { get; set; }

        public string Address { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public VendorStatus Status { get; set; } = VendorStatus.Draft;

        public string? StatusReason { get; set; }

        public decimal RatingAverage { get; set; }

        public int RatingsCount { get; set; }

        public DateTime? SubmittedForReviewAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties

        public ApplicationUser OwnerUser { get; set; } = null!;

        public Governorate Governorate { get; set; } = null!;

        public ICollection<VendorZone> ServiceZones { get; set; }
            = new List<VendorZone>();

        public ICollection<VendorDocument> Documents { get; set; }
            = new List<VendorDocument>();

        public ICollection<VendorPayoutAccount> PayoutAccounts { get; set; }
            = new List<VendorPayoutAccount>();

        public ICollection<VendorStatusHistory> StatusHistory { get; set; }
            = new List<VendorStatusHistory>();

        public ICollection<VendorCategory> Categories { get; set; }
    = new List<VendorCategory>();
    }
}