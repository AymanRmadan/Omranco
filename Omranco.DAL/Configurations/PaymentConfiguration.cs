using Omranco.Domain.Entities.Payments;

namespace Omranco.DAL.Configurations.Payments
{
    public sealed class PaymentConfiguration
        : IEntityTypeConfiguration<Payment>
    {
        public void Configure(
            EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.PaidAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.RefundedAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Order واحد له Payment واحدة
            builder.HasIndex(x => x.OrderId)
                .IsUnique();

            builder.HasOne(x => x.Order)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}