using Omranco.Domain.Entities.Catalog;

namespace Omranco.DAL.Configurations.Catalog
{
    public sealed class ProductAttributeOptionConfiguration
        : IEntityTypeConfiguration<ProductAttributeOption>
    {
        public void Configure(EntityTypeBuilder<ProductAttributeOption> builder)
        {
            builder.ToTable("ProductAttributeOptions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            builder.HasIndex(x => new
            {
                x.ProductAttributeId,
                x.Value
            })
            .IsUnique();

            builder.HasOne(x => x.ProductAttribute)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.ProductAttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}