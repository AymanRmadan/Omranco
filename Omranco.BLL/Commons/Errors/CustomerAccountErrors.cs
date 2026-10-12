namespace Omranco.BLL.Commons.Errors;

public static class CustomerAccountErrors
{
    public static readonly Error NotFound =
        new(
            "CustomerAccount.NotFound",
            "Customer account was not found.",
            404
        );

    public static readonly Error AccessDenied =
        new(
            "CustomerAccount.AccessDenied",
            "You do not have permission to manage this customer account.",
            400
        );

    public static readonly Error InvalidName =
        new(
            "CustomerAccount.InvalidName",
            "Customer account name is required.",
            400
        );

    public static readonly Error UserNotFound =
        new(
            "CustomerAccount.UserNotFound",
            "No registered user was found with this phone number.",
            404
        );

    public static readonly Error MemberAlreadyExists =
        new(
            "CustomerAccount.MemberAlreadyExists",
            "The user already belongs to this customer account.",
            400
        );

    public static readonly Error MemberNotFound =
        new(
            "CustomerAccount.MemberNotFound",
            "Customer account member was not found.",
            404
        );

    public static readonly Error OwnerCannotBeRemoved =
        new(
            "CustomerAccount.OwnerCannotBeRemoved",
            "The owner cannot be removed from the customer account.",
            400
        );
}