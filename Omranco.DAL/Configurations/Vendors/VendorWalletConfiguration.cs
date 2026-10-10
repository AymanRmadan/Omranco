using Omranco.Domain.Entities.Wallets;

namespace Omranco.DAL.Configurations.Wallets
{
    public sealed class VendorWalletConfiguration
        : IEntityTypeConfiguration<VendorWallet>
    {
        public void Configure(
            EntityTypeBuilder<VendorWallet> builder)
        {
            builder.ToTable("VendorWallets");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Balance)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalEarned)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalSettled)
                .HasPrecision(18, 2);

            builder.HasIndex(x => x.VendorId)
                .IsUnique();

            builder.HasOne(x => x.Vendor)
                .WithOne()
                .HasForeignKey<VendorWallet>(
                    x => x.VendorId
                )
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}