using Omranco.Domain.Entities.Payments;

namespace Omranco.DAL.Configurations.Payments
{
    public sealed class PaymentAttemptConfiguration
        : IEntityTypeConfiguration<PaymentAttempt>
    {
        public void Configure(
            EntityTypeBuilder<PaymentAttempt> builder)
        {
            builder.ToTable("PaymentAttempts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Method)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Reference)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.PaymobIntentionId)
                .HasMaxLength(200);

            builder.Property(x => x.PaymobOrderId)
                .HasMaxLength(200);

            builder.Property(x => x.PaymobTransactionId)
                .HasMaxLength(200);

            builder.Property(x => x.FailureCode)
                .HasMaxLength(100);

            builder.Property(x => x.FailureMessage)
                .HasMaxLength(1000);

            builder.HasIndex(x => x.Reference)
                .IsUnique();

            builder.HasIndex(x => x.PaymobIntentionId);

            builder.HasIndex(x => x.PaymobOrderId);

            builder.HasIndex(x => x.PaymobTransactionId);

            builder.HasOne(x => x.Payment)
                .WithMany(x => x.Attempts)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}