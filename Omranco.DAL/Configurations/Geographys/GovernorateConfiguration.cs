using Omranco.Domain.Entities.Geography;

namespace Omranco.DAL.Configurations.Geography
{
    public sealed class GovernorateConfiguration
        : IEntityTypeConfiguration<Governorate>
    {
        public void Configure(EntityTypeBuilder<Governorate> builder)
        {
            builder.ToTable("Governorates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.NameAr)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.NameEn)
                .HasMaxLength(150);

            builder.HasIndex(x => x.NameAr);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);
        }
    }
}