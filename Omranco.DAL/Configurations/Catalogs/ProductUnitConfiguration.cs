using Omranco.Domain.Entities.Catalog;

namespace Omranco.DAL.Configurations.Catalog
{
    public sealed class ProductUnitConfiguration
        : IEntityTypeConfiguration<ProductUnit>
    {
        public void Configure(EntityTypeBuilder<ProductUnit> builder)
        {
            builder.ToTable("ProductUnits");

            builder.HasKey(x => new
            {
                x.ProductId,
                x.UnitId
            });

            builder.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Units)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Unit)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}