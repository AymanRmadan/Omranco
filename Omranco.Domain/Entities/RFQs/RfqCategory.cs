using Omranco.Domain.Entities.Catalog;

namespace Omranco.Domain.Entities.Rfqs
{
    public sealed class RfqCategory : BaseEntity<int>
    {
        public int RfqId { get; set; }

        public int CategoryId { get; set; }


        // Navigation Properties

        public Rfq Rfq { get; set; } = null!;

        public Category Category { get; set; } = null!;
    }
}