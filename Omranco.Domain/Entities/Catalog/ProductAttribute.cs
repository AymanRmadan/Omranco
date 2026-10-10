namespace Omranco.Domain.Entities.Catalog
{
    public sealed class ProductAttribute : BaseEntity<int>
    {

        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = true;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;


        // Navigation Properties
        public Product Product { get; set; } = null!;

        public ICollection<ProductAttributeOption> Options { get; set; }
            = new List<ProductAttributeOption>();
    }
}