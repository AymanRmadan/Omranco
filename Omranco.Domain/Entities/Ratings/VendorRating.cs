using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Entities.Vendors;

namespace Omranco.Domain.Entities.Ratings
{
    public sealed class VendorRating : BaseEntity<int>
    {
        public int OrderId { get; set; }

        public int VendorId { get; set; }

        public int CustomerAccountUserId { get; set; }

        public int Stars { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;


        // Navigation Properties

        public Order Order { get; set; } = null!;

        public Vendor Vendor { get; set; } = null!;

        public CustomerAccountUser CustomerAccountUser { get; set; }
            = null!;
    }
}