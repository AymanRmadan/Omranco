namespace Omranco.Domain.Entities.Catalog
{
    public sealed class ProductAttributeOption : BaseEntity<int>
    {

        public int ProductAttributeId { get; set; }

        public string Value { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;


        // Navigation Properties
        public ProductAttribute ProductAttribute { get; set; } = null!;
    }
}