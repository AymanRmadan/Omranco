using Omranco.Domain.Entities.Orders;

namespace Omranco.DAL.Configurations.Orders;

public sealed class OrderDisputeConfiguration
    : IEntityTypeConfiguration<OrderDispute>
{
    public void Configure(
        EntityTypeBuilder<OrderDispute> builder)
    {
        builder.ToTable("OrderDisputes");

        builder.HasKey(x => x.Id);


        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();


        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();


        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();


        builder.Property(x => x.Description)
            .HasMaxLength(2000);


        builder.Property(x => x.ReviewNotes)
            .HasMaxLength(2000);


        builder.Property(x => x.ApprovedRefundAmount)
            .HasPrecision(18, 2);


        builder.Property(x => x.OpenedAt)
            .IsRequired();


        builder.HasIndex(x => new
        {
            x.OrderId,
            x.Status
        });


        builder.HasIndex(x => x.PaymentRefundId)
            .IsUnique()
            .HasFilter(
                "[PaymentRefundId] IS NOT NULL"
            );


        builder.HasOne(x => x.Order)
            .WithMany(x => x.Disputes)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(x => x.OpenedByAccountUser)
            .WithMany()
            .HasForeignKey(x => x.OpenedByAccountUserId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(x => x.ReviewedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(x => x.PaymentRefund)
            .WithOne()
            .HasForeignKey<OrderDispute>(
                x => x.PaymentRefundId
            )
            .OnDelete(DeleteBehavior.Restrict);
    }
}