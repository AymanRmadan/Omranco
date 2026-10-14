using Omranco.Domain.Entities.Catalog;

namespace Omranco.DAL.Configurations.Catalog
{
    public sealed class ProductVariantValueConfiguration
        : IEntityTypeConfiguration<ProductVariantValue>
    {
        public void Configure(EntityTypeBuilder<ProductVariantValue> builder)
        {
            builder.ToTable("ProductVariantValues");

            builder.HasKey(x => new
            {
                x.ProductVariantId,
                x.ProductAttributeId
            });

            builder.HasIndex(
    x => new
    {
        x.ProductVariantId,
        x.ProductAttributeId
    })
    .IsUnique();

            builder.HasOne(x => x.ProductVariant)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ProductAttribute)
                .WithMany()
                .HasForeignKey(x => x.ProductAttributeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductAttributeOption)
                .WithMany()
                .HasForeignKey(x => x.ProductAttributeOptionId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}