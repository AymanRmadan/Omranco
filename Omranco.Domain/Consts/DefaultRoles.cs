namespace Omranco.Domain;

public static class DefaultRoles
{
    public const string Owner = "Owner";

    public const string Admin = "Admin";

    public static readonly string[] All =
    [
        Owner,
        Admin
    ];
}