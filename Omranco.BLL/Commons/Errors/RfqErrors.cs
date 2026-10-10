namespace Omranco.BLL.Commons.Errors
{
    public static class RfqErrors
    {
        public static readonly Error NotFound =
            new("Rfq.NotFound", "RFQ not found.", 404);

        public static readonly Error NotDraft =
            new("Rfq.NotDraft", "Only draft RFQs can be submitted.", 404);

        public static readonly Error UnauthorizedProjectAccess =
            new(
                "Rfq.UnauthorizedProjectAccess",
                "You do not have access to this project."
                , 403
            );

        public static readonly Error CategoriesRequired =
            new("Rfq.CategoriesRequired", "RFQ must contain at least one category.", 400);


        public static readonly Error ContentRequired =
            new("Rfq.ContentRequired", "RFQ must contain at least one item or attachment.", StatusCodes.Status400BadRequest);

        public static readonly Error InvalidDeadline =
            new("Rfq.InvalidDeadline", "RFQ deadline must be in the future.", 401);
    }
}