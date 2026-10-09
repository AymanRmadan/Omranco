using Omranco.Domain.Entities.Projects;

namespace Omranco.DAL.Configurations.Projects
{
    public sealed class ProjectMemberConfiguration
        : IEntityTypeConfiguration<ProjectMember>
    {
        public void Configure(EntityTypeBuilder<ProjectMember> builder)
        {
            builder.ToTable("ProjectMembers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.AssignedAt)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ProjectId,
                x.CustomerAccountUserId
            })
            .IsUnique();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CustomerAccountUser)
                .WithMany(x => x.ProjectMemberships)
                .HasForeignKey(x => x.CustomerAccountUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedByAccountUser)
                .WithMany(x => x.AssignedProjectMembers)
                .HasForeignKey(x => x.AssignedByAccountUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}