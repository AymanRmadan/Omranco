using Omranco.Domain.Entities.Vendors;

namespace Omranco.DAL.Configurations.Vendors
{
    public sealed class VendorCategoryConfiguration
        : IEntityTypeConfiguration<VendorCategory>
    {
        public void Configure(EntityTypeBuilder<VendorCategory> builder)
        {
            builder.ToTable("VendorCategories");

            builder.HasKey(x => new
            {
                x.VendorId,
                x.CategoryId
            });

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.AddedAt)
                .IsRequired();

            builder.HasOne(x => x.Vendor)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Vendors)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}