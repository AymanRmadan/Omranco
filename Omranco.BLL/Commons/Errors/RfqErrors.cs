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

        public static readonly Error AccessDenied =
    new(
        "Rfq.AccessDenied",
        "You do not have access to this RFQ.",
                400
    );

        public static readonly Error ProjectNotFound =
            new(
                "Rfq.ProjectNotFound",
                "Project was not found.",
                404
            );


        public static readonly Error InvalidDeliveryDate =
            new(
                "Rfq.InvalidDeliveryDate",
                "Desired delivery date cannot be in the past.",
                400
            );

        public static readonly Error InvalidQuantity =
            new(
                "Rfq.InvalidQuantity",
                "RFQ item quantity must be greater than zero.",
                400
            );

        public static readonly Error ProductNotFound =
            new(
                "Rfq.ProductNotFound",
                "One or more selected products were not found.",
                404
            );

        public static readonly Error VariantNotFound =
            new(
                "Rfq.VariantNotFound",
                "The selected product variant is invalid.",
                404
            );

        public static readonly Error UnitNotFound =
            new(
                "Rfq.UnitNotFound",
                "One or more selected units were not found.",
                404
            );

        public static readonly Error InvalidCategory =
            new(
                "Rfq.InvalidCategory",
                "One or more selected categories were not found.",
                404
            );

        public static readonly Error OnlyDraftCanBeEdited =
            new(
                "Rfq.OnlyDraftCanBeEdited",
                "Only draft RFQs can be edited.",
                400
            );

        public static readonly Error InvalidStatus =
            new(
                "Rfq.InvalidStatus",
                "The RFQ cannot be updated in its current status.",
                400
            );

        public static readonly Error AttachmentNotFound =
            new(
                "Rfq.AttachmentNotFound",
                "RFQ attachment was not found.",
                404
            );

        public static readonly Error AttachmentTooLarge =
            new(
                "Rfq.AttachmentTooLarge",
                "RFQ attachment exceeds the maximum allowed size.",
                400
            );

        public static readonly Error UnsupportedAttachment =
            new(
                "Rfq.UnsupportedAttachment",
                "The selected attachment type is not supported.",
                400
            );
    }
}