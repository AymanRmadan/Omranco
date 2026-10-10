namespace Omranco.Domain;

public static class DefaultRoles
{
    public const string Admin = nameof(Admin);
    public const int AdminRoleId = 1;
    public const string AdminRoleConcurrencyStamp = "981ee2ee-8328-48ca-9ea7-012b4fab1888";

    public const string Owner = nameof(Owner);
    public const int OwnerRoleId = 2;
    public const string OwnerRoleConcurrencyStamp = "4ee6bc12-5cb0-4304-91e7-6a00744e042b";

    public const string Member = nameof(Member);
    public const int MemberRoleId = 3;
    public const string MemberRoleConcurrencyStamp = "3ee6bc12-5cb0-4304-91e7-6a00744e042c";
}