using Omranco.Domain.Entities.Catalog;

namespace Omranco.DAL.Configurations.Catalog
{
    public sealed class ProductAttributeConfiguration
        : IEntityTypeConfiguration<ProductAttribute>
    {
        public void Configure(EntityTypeBuilder<ProductAttribute> builder)
        {
            builder.ToTable("ProductAttributes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.IsRequired)
                .HasDefaultValue(true);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            builder.HasIndex(x => new
            {
                x.ProductId,
                x.Name
            })
            .IsUnique();

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Attributes)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}