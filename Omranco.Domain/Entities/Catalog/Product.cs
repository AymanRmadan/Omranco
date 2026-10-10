namespace Omranco.Domain.Entities.Catalog
{
    public sealed class Product : BaseEntity<int>
    {

        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties
        public Category Category { get; set; } = null!;

        public ICollection<ProductAttribute> Attributes { get; set; }
            = new List<ProductAttribute>();

        public ICollection<ProductVariant> Variants { get; set; }
            = new List<ProductVariant>();

        public ICollection<ProductUnit> Units { get; set; }
            = new List<ProductUnit>();
    }
}