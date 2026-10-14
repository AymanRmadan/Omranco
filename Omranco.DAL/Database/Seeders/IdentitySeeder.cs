using Omranco.Domain;
using Omranco.Domain.Entities.Auths;

namespace Omranco.DAL.Database.Seeders;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        using var scope =
            serviceProvider.CreateScope();


        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<
                    RoleManager<ApplicationRole>
                >();


        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>
                >();


        // ==========================================
        // Roles
        // ==========================================

        foreach (var roleName in DefaultRoles.All)
        {
            if (
                await roleManager
                    .RoleExistsAsync(
                        roleName
                    )
            )
            {
                continue;
            }


            var result =
                await roleManager
                    .CreateAsync(
                        new ApplicationRole
                        {
                            Name =
                                roleName
                        }
                    );


            if (!result.Succeeded)
            {
                var message =
                    string.Join(
                        " | ",
                        result.Errors
                            .Select(
                                x =>
                                    $"{x.Code}: {x.Description}"
                            )
                    );


                throw new InvalidOperationException(
                    $"Failed to seed role '{roleName}'. {message}"
                );
            }
        }


        // ==========================================
        // Optional first Admin
        // ==========================================

        var adminEmail =
            configuration[
                "AdminSeed:Email"
            ];


        var adminPassword =
            configuration[
                "AdminSeed:Password"
            ];


        var adminName =
            configuration[
                "AdminSeed:Name"
            ]
            ?? "Omranco Admin";


        /*
         * If no AdminSeed configuration exists,
         * only Roles are seeded.
         */
        if (
            string.IsNullOrWhiteSpace(
                adminEmail
            )
            ||
            string.IsNullOrWhiteSpace(
                adminPassword
            )
        )
        {
            return;
        }


        var admin =
            await userManager
                .FindByEmailAsync(
                    adminEmail
                );


        if (admin is null)
        {
            admin =
                new ApplicationUser
                {
                    Email =
                        adminEmail,

                    UserName =
                        adminEmail,

                    Name =
                        adminName,

                    EmailConfirmed =
                        true,

                    IsActive =
                        true
                };


            var createResult =
                await userManager
                    .CreateAsync(
                        admin,
                        adminPassword
                    );


            if (!createResult.Succeeded)
            {
                var message =
                    string.Join(
                        " | ",
                        createResult.Errors
                            .Select(
                                x =>
                                    $"{x.Code}: {x.Description}"
                            )
                    );


                throw new InvalidOperationException(
                    $"Failed to seed admin user. {message}"
                );
            }
        }


        if (
            !await userManager
                .IsInRoleAsync(
                    admin,
                    DefaultRoles.Admin
                )
        )
        {
            var roleResult =
                await userManager
                    .AddToRoleAsync(
                        admin,
                        DefaultRoles.Admin
                    );


            if (!roleResult.Succeeded)
            {
                var message =
                    string.Join(
                        " | ",
                        roleResult.Errors
                            .Select(
                                x =>
                                    $"{x.Code}: {x.Description}"
                            )
                    );


                throw new InvalidOperationException(
                    $"Failed to assign Admin role. {message}"
                );
            }
        }
    }
}