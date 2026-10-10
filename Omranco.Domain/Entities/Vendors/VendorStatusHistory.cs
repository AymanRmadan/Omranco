using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Enums.Vendors;

namespace Omranco.Domain.Entities.Vendors
{
    public sealed class VendorStatusHistory : BaseEntity<int>
    {

        public int VendorId { get; set; }

        public VendorStatus? OldStatus { get; set; }

        public VendorStatus NewStatus { get; set; }

        public string? Reason { get; set; }

        public int? ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;


        public Vendor Vendor { get; set; } = null!;

        public ApplicationUser? ChangedByUser { get; set; }
    }
}