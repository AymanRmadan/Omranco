using Omranco.Domain.Entities.Rfqs;

namespace Omranco.Domain.Entities.Bids
{
    public sealed class BidItem : BaseEntity<int>
    {
        public int BidId { get; set; }

        public int RfqItemId { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public string? Notes { get; set; }


        // Navigation Properties

        public Bid Bid { get; set; } = null!;

        public RfqItem RfqItem { get; set; } = null!;
    }
}