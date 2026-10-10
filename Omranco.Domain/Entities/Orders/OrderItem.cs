using Omranco.Domain.Entities.Catalog;
using Omranco.Domain.Entities.Rfqs;

namespace Omranco.Domain.Entities.Orders
{
    public sealed class OrderItem : BaseEntity<int>
    {
        public int OrderId { get; set; }

        public int RfqItemId { get; set; }

        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        public int UnitId { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }


        public Order Order { get; set; } = null!;

        public RfqItem RfqItem { get; set; } = null!;

        public Product Product { get; set; } = null!;

        public ProductVariant? ProductVariant { get; set; }

        public Unit Unit { get; set; } = null!;
    }
}