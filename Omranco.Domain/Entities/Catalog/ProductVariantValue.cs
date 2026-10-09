namespace Omranco.Domain.Entities.Catalog
{
    public sealed class ProductVariantValue
    {
        public int ProductVariantId { get; set; }

        public int ProductAttributeId { get; set; }

        public int ProductAttributeOptionId { get; set; }


        // Navigation Properties
        public ProductVariant ProductVariant { get; set; } = null!;

        public ProductAttribute ProductAttribute { get; set; } = null!;

        public ProductAttributeOption ProductAttributeOption { get; set; } = null!;
    }
}