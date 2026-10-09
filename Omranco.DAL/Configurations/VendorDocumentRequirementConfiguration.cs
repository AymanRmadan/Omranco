using Omranco.Domain.Entities.Vendors;

namespace Omranco.DAL.Configurations.Vendors
{
    public sealed class VendorDocumentRequirementConfiguration
        : IEntityTypeConfiguration<VendorDocumentRequirement>
    {
        public void Configure(
            EntityTypeBuilder<VendorDocumentRequirement> builder)
        {
            builder.ToTable("VendorDocumentRequirements");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.VendorType)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.IsRequired)
                .HasDefaultValue(true);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);
        }
    }
}