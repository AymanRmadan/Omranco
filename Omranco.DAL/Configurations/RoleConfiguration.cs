using Omranco.Domain;
using Omranco.Domain.Entities.Auths;

namespace Omranco.DAL
{
    public class RoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            // Default Data
            builder.HasData([
                new ApplicationRole
                {
                    Id = DefaultRoles.AdminRoleId,
                    Name = DefaultRoles.Admin,
                    NormalizedName = DefaultRoles.Admin.ToUpper(),
                    ConcurrencyStamp = DefaultRoles.AdminRoleConcurrencyStamp
                },
                new ApplicationRole
                {
                    Id = DefaultRoles.ManagerRoleId,
                    Name = DefaultRoles.Manager,
                    NormalizedName = DefaultRoles.Manager.ToUpper(),
                    ConcurrencyStamp = DefaultRoles.ManagerRoleConcurrencyStamp
                },
                new ApplicationRole
                {
                    Id = DefaultRoles.LearnerRoleId,
                    Name = DefaultRoles.Learner,
                    NormalizedName = DefaultRoles.Learner.ToUpper(),
                    ConcurrencyStamp = DefaultRoles.LearnerRoleConcurrencyStamp
                }
            ]);
        }
    }
}