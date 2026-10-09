using Omranco.Domain.Entities.Projects;

namespace Omranco.DAL.Configurations.Projects
{
    public sealed class ProjectConfiguration
        : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Address)
                .HasMaxLength(500);

            builder.Property(x => x.Latitude)
                .HasPrecision(10, 7);

            builder.Property(x => x.Longitude)
                .HasPrecision(10, 7);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.CustomerAccount)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.CustomerAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CustomerAccountId);
        }
    }
}