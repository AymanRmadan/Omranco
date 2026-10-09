using Omranco.Domain.Entities.Auths;

namespace Omranco.DAL.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            //builder.OwnsMany(u => u.RefreshTokens).ToTable("RefreshTokens")
            //    .WithOwner().HasForeignKey("UserId");

            builder.OwnsMany(u => u.RefreshTokens, rt =>
            {
                rt.ToTable("RefreshTokens");
                rt.WithOwner().HasForeignKey("UserId");
                rt.Property<int>("UserId");
            });



            //builder.HasData([
            //     new ApplicationUser
            //    {
            //        Id = DefaultUsers.AdminId,
            //        UserName = "admin",
            //        NormalizedUserName = "Admin",
            //        Email = "admin@gmail.com",
            //        NormalizedEmail = "ADMIN@GMAIL.COM",
            //        EmailConfirmed = true,
            //        Name = "admin",

            //        SecurityStamp = "b3c8f352-7a2e-4b61-9c8a-7235aef214db",
            //        ConcurrencyStamp = "c4d9e163-8b3f-5c72-0d9b-8346bfa325ec",

            //        PasswordHash = DefaultUsers.DefaultPasswordHash
            //    },
            //    new ApplicationUser
            //    {
            //        Id = DefaultUsers.ManagerId,
            //        UserName = "manager",
            //        NormalizedUserName = "MANAGER",
            //        Email = "manager@gmail.com",
            //        NormalizedEmail = "MANAGER@GMAIL.COM",
            //        EmailConfirmed = true,
            //        Name = "Manager",
            //        SecurityStamp = "66BF92C9EF0249CDA210D85D1A851BC0",
            //        ConcurrencyStamp = "cabd18ab-f314-4be5-ab70-efbb221639e0",
            //        PasswordHash = DefaultUsers.DefaultPasswordHash
            //    }
            // ]);
        }
    }
}