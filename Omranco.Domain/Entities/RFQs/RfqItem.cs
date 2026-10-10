using Omranco.Domain.Entities.Catalog;

namespace Omranco.Domain.Entities.Rfqs
{
    public sealed class RfqItem : BaseEntity<int>
    {

        public int RfqId { get; set; }

        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        public int UnitId { get; set; }

        public decimal Quantity { get; set; }

        public string? Notes { get; set; }


        // Navigation Properties

        public Rfq Rfq { get; set; } = null!;

        public Product Product { get; set; } = null!;

        public ProductVariant? ProductVariant { get; set; }

        public Unit Unit { get; set; } = null!;
    }
}