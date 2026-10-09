namespace Omranco.Domain.Entities.Catalog
{
    public sealed class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string? Name { get; set; }

        public string? Sku { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties
        public Product Product { get; set; } = null!;

        public ICollection<ProductVariantValue> Values { get; set; }
            = new List<ProductVariantValue>();
    }
}