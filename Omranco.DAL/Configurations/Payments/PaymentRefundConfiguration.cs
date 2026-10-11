using Omranco.Domain.Entities.Payments;

namespace Omranco.DAL.Configurations.Payments;

public sealed class PaymentRefundConfiguration
    : IEntityTypeConfiguration<PaymentRefund>
{
    public void Configure(
        EntityTypeBuilder<PaymentRefund> builder)
    {
        builder.ToTable("PaymentRefunds");

        builder.HasKey(x => x.Id);


        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();


        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();


        builder.Property(x => x.Reference)
            .HasMaxLength(100)
            .IsRequired();


        builder.Property(x => x.Reason)
            .HasMaxLength(1000);


        builder.Property(x => x.PaymobRefundTransactionId)
            .HasMaxLength(200);


        builder.Property(x => x.FailureCode)
            .HasMaxLength(100);


        builder.Property(x => x.FailureMessage)
            .HasMaxLength(1000);


        builder.Property(x => x.RequestedAt)
            .IsRequired();


        builder.HasIndex(x => x.Reference)
            .IsUnique();


        builder.HasIndex(x => x.PaymobRefundTransactionId)
            .IsUnique()
            .HasFilter(
                "[PaymobRefundTransactionId] IS NOT NULL"
            );


        builder.HasIndex(x => new
        {
            x.PaymentId,
            x.Status
        });


        builder.HasOne(x => x.Payment)
            .WithMany(x => x.Refunds)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(x => x.PaymentAttempt)
            .WithMany(x => x.Refunds)
            .HasForeignKey(x => x.PaymentAttemptId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}