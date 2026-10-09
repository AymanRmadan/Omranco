using Omranco.Domain.Entities.Vendors;

namespace Omranco.DAL.Configurations.Vendors
{
    public sealed class VendorZoneConfiguration
        : IEntityTypeConfiguration<VendorZone>
    {
        public void Configure(EntityTypeBuilder<VendorZone> builder)
        {
            builder.ToTable("VendorZones");

            builder.HasKey(x => new
            {
                x.VendorId,
                x.ZoneId
            });

            builder.HasOne(x => x.Vendor)
                .WithMany(x => x.ServiceZones)
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Zone)
                .WithMany()
                .HasForeignKey(x => x.ZoneId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}