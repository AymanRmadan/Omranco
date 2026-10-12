namespace Omranco.BLL.Commons.Errors;

public static class ProjectErrors
{
    public static readonly Error NotFound =
        new(
            "Project.NotFound",
            "Project was not found.",
            404
        );

    public static readonly Error AccessDenied =
        new(
            "Project.AccessDenied",
            "You do not have permission to manage this project.",
            400
        );

    public static readonly Error InvalidName =
        new(
            "Project.InvalidName",
            "Project name is required.",
            400
        );

    public static readonly Error ZoneNotFound =
        new(
            "Project.ZoneNotFound",
            "The selected zone was not found.",
            404
        );

    public static readonly Error MemberNotInAccount =
        new(
            "Project.MemberNotInAccount",
            "The selected member does not belong to the project's customer account.",
            400
        );

    public static readonly Error MemberAlreadyAssigned =
        new(
            "Project.MemberAlreadyAssigned",
            "The member is already assigned to this project.",
            400
        );

    public static readonly Error ProjectMemberNotFound =
        new(
            "Project.ProjectMemberNotFound",
            "The project member assignment was not found.",
            404
        );
}