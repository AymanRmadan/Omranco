using Omranco.Domain.Entities.Rfqs;

namespace Omranco.DAL.Configurations.Rfqs
{
    public sealed class RfqConfiguration
        : IEntityTypeConfiguration<Rfq>
    {
        public void Configure(EntityTypeBuilder<Rfq> builder)
        {
            builder.ToTable("Rfqs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Number)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.Number)
                .IsUnique();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasMaxLength(2000);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.ProjectId);

            builder.HasIndex(x => new
            {
                x.Status,
                x.DeadlineAt
            });

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByAccountUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByAccountUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
