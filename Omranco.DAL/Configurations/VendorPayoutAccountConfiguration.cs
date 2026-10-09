using Omranco.Domain.Entities.Vendors;

namespace Omranco.DAL.Configurations.Vendors
{
    public sealed class VendorPayoutAccountConfiguration
        : IEntityTypeConfiguration<VendorPayoutAccount>
    {
        public void Configure(EntityTypeBuilder<VendorPayoutAccount> builder)
        {
            builder.ToTable("VendorPayoutAccounts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Method)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.AccountHolderName)
                .HasMaxLength(200);

            builder.Property(x => x.BankName)
                .HasMaxLength(200);

            builder.Property(x => x.AccountNumber)
                .HasMaxLength(100);

            builder.Property(x => x.Iban)
                .HasMaxLength(100);

            builder.Property(x => x.WalletProvider)
                .HasMaxLength(100);

            builder.Property(x => x.WalletPhoneNumber)
                .HasMaxLength(30);

            builder.HasOne(x => x.Vendor)
                .WithMany(x => x.PayoutAccounts)
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}