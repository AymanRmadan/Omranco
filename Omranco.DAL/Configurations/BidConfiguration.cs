using Omranco.Domain.Entities.Bids;

namespace Omranco.DAL.Configurations.Bids
{
    public sealed class BidConfiguration
        : IEntityTypeConfiguration<Bid>
    {
        public void Configure(EntityTypeBuilder<Bid> builder)
        {
            builder.ToTable("Bids");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ShippingCostType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Subtotal)
                .HasPrecision(18, 2);

            builder.Property(x => x.ShippingCost)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.PaymentTerms)
                .HasMaxLength(1000);

            builder.Property(x => x.Notes)
                .HasMaxLength(2000);

            builder.HasIndex(
        x => new
        {
            x.RfqId,
            x.VendorId
        }
    )
    .IsUnique();

            builder.HasOne(x => x.Rfq)
                .WithMany()
                .HasForeignKey(x => x.RfqId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Vendor)
                .WithMany()
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.RowVersion).IsRowVersion();
        }
    }
}