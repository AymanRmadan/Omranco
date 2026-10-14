using Omranco.Domain.Entities.Catalog;

namespace Omranco.DAL.Configurations.Catalog
{
    public sealed class ProductVariantConfiguration
        : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(300);

            builder.Property(x => x.Sku)
                .HasMaxLength(100);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.ProductId);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.Sku).IsUnique().HasFilter("[Sku] IS NOT NULL");
        }
    }
}