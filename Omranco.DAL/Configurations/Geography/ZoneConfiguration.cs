using Omranco.Domain.Entities.Geography;

namespace Omranco.DAL.Configurations.Geography
{
    public sealed class ZoneConfiguration
        : IEntityTypeConfiguration<Zone>
    {
        public void Configure(EntityTypeBuilder<Zone> builder)
        {
            builder.ToTable("Zones");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.NameAr)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.NameEn)
                .HasMaxLength(150);

            builder.HasOne(x => x.Governorate)
                .WithMany(x => x.Zones)
                .HasForeignKey(x => x.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.GovernorateId,
                x.NameAr
            })
            .IsUnique();
        }
    }
}