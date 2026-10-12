using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Entities.Orders;

namespace Omranco.Domain.Entities.Vendors
{
    public sealed class VendorRating : BaseEntity<int>
    {
        public int OrderId { get; set; }

        public int VendorId { get; set; }

        public int RatedByUserId { get; set; }

        public int Stars { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;


        // Navigation Properties

        public Order Order { get; set; } = null!;

        public Vendor Vendor { get; set; } = null!;

        public ApplicationUser RatedByUser { get; set; } = null!;
    }
}