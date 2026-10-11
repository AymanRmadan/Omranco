using Omranco.Domain.Entities.Orders;

namespace Omranco.DAL.Configurations.Orders;

public sealed class OrderCancellationConfiguration
    : IEntityTypeConfiguration<OrderCancellation>
{
    public void Configure(
        EntityTypeBuilder<OrderCancellation> builder)
    {
        builder.ToTable("OrderCancellations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(1000);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(1000);

        builder.Property(x => x.RequestedAt)
            .IsRequired();

        builder.HasIndex(x => x.OrderId)
            .IsUnique();

        builder.HasIndex(x => x.PaymentRefundId)
            .IsUnique()
            .HasFilter("[PaymentRefundId] IS NOT NULL");

        builder.HasOne(x => x.Order)
            .WithOne(x => x.Cancellation)
            .HasForeignKey<OrderCancellation>(
                x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentRefund)
            .WithOne()
            .HasForeignKey<OrderCancellation>(
                x => x.PaymentRefundId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}