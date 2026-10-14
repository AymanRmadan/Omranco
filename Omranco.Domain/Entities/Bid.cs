using Omranco.Domain.Entities.Rfqs;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Enums.Bids;

namespace Omranco.Domain.Entities.Bids
{
    public sealed class Bid : BaseEntity<int>
    {
        public int RfqId { get; set; }

        public int VendorId { get; set; }

        public BidStatus Status { get; set; }
            = BidStatus.Submitted;

        public decimal Subtotal { get; set; }

        public ShippingCostType ShippingCostType { get; set; }

        public decimal ShippingCost { get; set; }

        public decimal TotalAmount { get; set; }

        public int LeadTimeDays { get; set; }

        public string? PaymentTerms { get; set; }

        public DateTime ValidUntil { get; set; }

        public string? Notes { get; set; }

        public DateTime SubmittedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public DateTime? WithdrawnAt { get; set; }
        public DateTime? AcceptedAt { get; set; }

        public DateTime? RejectedAt { get; set; }


        // Navigation Properties

        public Rfq Rfq { get; set; } = null!;

        public Vendor Vendor { get; set; } = null!;

        public ICollection<BidItem> Items { get; set; }
            = new List<BidItem>();
    }
}