using Omranco.Domain.Entities.Vendors;

namespace Omranco.DAL.Configurations.Vendors
{
    public sealed class VendorConfiguration
        : IEntityTypeConfiguration<Vendor>
    {
        public void Configure(EntityTypeBuilder<Vendor> builder)
        {
            builder.ToTable("Vendors");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BusinessName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.StatusReason)
                .HasMaxLength(500);

            builder.Property(x => x.Latitude)
                .HasPrecision(10, 7);

            builder.Property(x => x.Longitude)
                .HasPrecision(10, 7);

            builder.Property(x => x.RatingAverage)
                .HasPrecision(3, 2);

            builder.HasIndex(x => x.OwnerUserId)
                .IsUnique();

            builder.HasOne(x => x.OwnerUser)
                .WithOne(x => x.OwnedVendor)
                .HasForeignKey<Vendor>(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Governorate)
                .WithMany()
                .HasForeignKey(x => x.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}