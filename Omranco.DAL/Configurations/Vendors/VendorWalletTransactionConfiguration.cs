using Omranco.Domain.Entities.Wallets;

namespace Omranco.DAL.Configurations.Wallets
{
    public sealed class VendorWalletTransactionConfiguration
        : IEntityTypeConfiguration<VendorWalletTransaction>
    {
        public void Configure(
            EntityTypeBuilder<VendorWalletTransaction> builder)
        {
            builder.ToTable("VendorWalletTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.HasIndex(x => x.Reference)
    .IsUnique();

            builder.Property(x => x.Direction)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2);

            builder.Property(x => x.BalanceBefore)
                .HasPrecision(18, 2);

            builder.Property(x => x.BalanceAfter)
                .HasPrecision(18, 2);

            builder.Property(x => x.Reference)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.HasIndex(x => x.Reference)
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.VendorWalletId,
                x.CreatedAt
            });

            builder.HasOne(x => x.VendorWallet)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.VendorWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}