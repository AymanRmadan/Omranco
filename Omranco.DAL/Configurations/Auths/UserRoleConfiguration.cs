namespace Omranco.DAL.Configurations.Auths
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<int>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<int>> builder)
        {
            //builder.HasData([
            //    new IdentityUserRole<int>
            //    {
            //        UserId = DefaultUsers.AdminId,
            //        RoleId = DefaultRoles.AdminRoleId
            //    },
            //    new IdentityUserRole<int>
            //    {
            //        UserId = DefaultUsers.ManagerId,
            //        RoleId = DefaultRoles.ManagerRoleId
            //    }
            //]);
        }
    }
}