using Omranco.Domain.Entities.Vendors;

namespace Omranco.Domain.Entities.Catalog
{
    public sealed class Category
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImagePath { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties
        public Category? Parent { get; set; }

        public ICollection<Category> Children { get; set; }
            = new List<Category>();

        public ICollection<Product> Products { get; set; }
            = new List<Product>();

        public ICollection<VendorCategory> Vendors { get; set; }
    = new List<VendorCategory>();
    }
}