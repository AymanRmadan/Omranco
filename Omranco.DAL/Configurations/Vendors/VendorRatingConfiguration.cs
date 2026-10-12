using Omranco.Domain.Entities.Vendors;

namespace Omranco.DAL.Configurations.Vendors
{
    public sealed class VendorRatingConfiguration
        : IEntityTypeConfiguration<VendorRating>
    {
        public void Configure(
            EntityTypeBuilder<VendorRating> builder)
        {
            builder.ToTable(
                "VendorRatings"
            );


            builder.HasKey(
                x => x.Id
            );


            builder.Property(
                    x => x.Stars
                )
                .IsRequired();


            builder.Property(
                    x => x.Comment
                )
                .HasMaxLength(
                    1000
                );


            builder.Property(
                    x => x.CreatedAt
                )
                .IsRequired();


            // ==========================================
            // One rating per Order
            // ==========================================

            builder.HasIndex(
                    x => x.OrderId
                )
                .IsUnique();


            builder.HasIndex(
                x => new
                {
                    x.VendorId,
                    x.CreatedAt
                }
            );


            // ==========================================
            // Relationships
            // ==========================================

            builder.HasOne(
                    x => x.Order
                )
                .WithMany()
                .HasForeignKey(
                    x => x.OrderId
                )
                .OnDelete(
                    DeleteBehavior.Restrict
                );


            builder.HasOne(
                    x => x.Vendor
                )
                .WithMany()
                .HasForeignKey(
                    x => x.VendorId
                )
                .OnDelete(
                    DeleteBehavior.Restrict
                );


            builder.HasOne(
                    x => x.RatedByUser
                )
                .WithMany()
                .HasForeignKey(
                    x => x.RatedByUserId
                )
                .OnDelete(
                    DeleteBehavior.Restrict
                );
        }
    }
}