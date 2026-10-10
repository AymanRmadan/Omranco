using Omranco.Domain.Entities.Geography;

namespace Omranco.Domain.Entities.Vendors
{
    public sealed class VendorZone : BaseEntity<int>
    {
        public int VendorId { get; set; }

        public int ZoneId { get; set; }

        public Vendor Vendor { get; set; } = null!;

        public Zone Zone { get; set; } = null!;
    }
}