using Omranco.Domain.Entities.Orders;

namespace Omranco.DAL.Configurations.Orders
{
    public sealed class OrderConfiguration
        : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Number)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.Number)
                .IsUnique();

            builder.HasIndex(x => x.RfqId)
                .IsUnique();

            builder.HasIndex(x => x.BidId)
                .IsUnique();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Subtotal)
                .HasPrecision(18, 2);

            builder.Property(x => x.ShippingCost)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.HasOne(x => x.Rfq)
                .WithOne()
                .HasForeignKey<Order>(x => x.RfqId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Bid)
                .WithOne()
                .HasForeignKey<Order>(x => x.BidId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Vendor)
                .WithMany()
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AcceptedByAccountUser)
                .WithMany()
                .HasForeignKey(x => x.AcceptedByAccountUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.CommissionRate)
    .HasPrecision(5, 2);

            builder.Property(x => x.CommissionAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.VendorNetAmount)
                .HasPrecision(18, 2);
        }
    }
}