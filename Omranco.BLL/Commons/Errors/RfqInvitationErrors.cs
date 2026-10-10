namespace Omranco.BLL.Commons.Errors
{
    public static class RfqInvitationErrors
    {
        public static readonly Error NotFound =
            new(
                    "RfqInvitation.NotFound",
                    "RFQ invitation not found.",
                    404
                );

        public static readonly Error AccessDenied =
            new(
                "RfqInvitation.AccessDenied",
                "You do not have access to this invitation.",
                400
            );

        public static readonly Error RfqNotOpen =
            new(
                "RfqInvitation.RfqNotOpen",
                "This RFQ is no longer open.",
                400
            );
    }
}

