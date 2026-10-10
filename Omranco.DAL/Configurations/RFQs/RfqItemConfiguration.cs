using Omranco.Domain.Entities.Rfqs;

namespace Omranco.DAL.Configurations.Rfqs
{
    public sealed class RfqItemConfiguration
        : IEntityTypeConfiguration<RfqItem>
    {
        public void Configure(EntityTypeBuilder<RfqItem> builder)
        {
            builder.ToTable("RfqItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.HasIndex(x => x.RfqId);

            builder.HasOne(x => x.Rfq)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.RfqId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Unit)
                .WithMany()
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}