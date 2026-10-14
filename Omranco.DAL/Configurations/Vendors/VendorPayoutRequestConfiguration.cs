using Omranco.Domain.Entities.Vendors;

namespace Omranco.DAL.Configurations.Vendors;

public sealed class VendorPayoutRequestConfiguration : IEntityTypeConfiguration<VendorPayoutRequest>
{
    public void Configure(
        EntityTypeBuilder<VendorPayoutRequest> builder)
    {
        builder.ToTable("VendorPayoutRequests");

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


        builder.Property(x => x.PayoutMethod)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();


        builder.Property(x => x.AccountHolderName)
            .HasMaxLength(200);

        builder.Property(x => x.BankName)
            .HasMaxLength(200);

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(100);

        builder.Property(x => x.IBAN)
            .HasMaxLength(100);

        builder.Property(x => x.WalletProvider)
            .HasMaxLength(100);

        builder.Property(x => x.WalletPhoneNumber)
            .HasMaxLength(50);

        builder.Property(x => x.ReviewNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.ExternalReference)
            .HasMaxLength(200);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(1000);


        builder.Property(x => x.RequestedAt)
            .IsRequired();


        builder.HasIndex(x => x.Reference)
            .IsUnique();


        builder.HasIndex(x => new
        {
            x.VendorId,
            x.Status
        });


        builder.HasIndex(x => x.ExternalReference);


        builder.HasOne(x => x.Vendor)
            .WithMany(x => x.PayoutRequests)
            .HasForeignKey(x => x.VendorId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(x => x.VendorPayoutAccount)
            .WithMany(x => x.PayoutRequests)
            .HasForeignKey(x => x.VendorPayoutAccountId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(x => x.ReviewedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}