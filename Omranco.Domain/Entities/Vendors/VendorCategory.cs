using Omranco.Domain.Entities.Catalog;

namespace Omranco.Domain.Entities.Vendors
{
    public sealed class VendorCategory : BaseEntity<int>
    {
        public int VendorId { get; set; }

        public int CategoryId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties
        public Vendor Vendor { get; set; } = null!;

        public Category Category { get; set; } = null!;
    }
}