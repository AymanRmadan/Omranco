using Omranco.Domain.Entities.Ratings;

namespace Omranco.DAL.Configurations.Ratings
{
    public sealed class VendorRatingConfiguration
        : IEntityTypeConfiguration<VendorRating>
    {
        public void Configure(
            EntityTypeBuilder<VendorRating> builder)
        {
            builder.ToTable("VendorRatings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Comment)
                .HasMaxLength(1000);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Order واحد = Rating واحد فقط
            builder.HasIndex(x => x.OrderId)
                .IsUnique();

            builder.HasIndex(x => x.VendorId);

            builder.HasOne(x => x.Order)
                .WithOne()
                .HasForeignKey<VendorRating>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Vendor)
                .WithMany()
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CustomerAccountUser)
                .WithMany()
                .HasForeignKey(x => x.CustomerAccountUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}