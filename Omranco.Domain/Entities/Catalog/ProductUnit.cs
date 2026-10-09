namespace Omranco.Domain.Entities.Catalog
{
    public sealed class ProductUnit
    {
        public int ProductId { get; set; }

        public int UnitId { get; set; }

        public bool IsDefault { get; set; }


        // Navigation Properties
        public Product Product { get; set; } = null!;

        public Unit Unit { get; set; } = null!;
    }
}