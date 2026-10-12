namespace Omranco.BLL.Commons.Errors
{
    public static class VendorErrors
    {
        public static readonly Error NotFound =
            new(
                "Vendor.NotFound",
                "Vendor account not found.",
                404
            );

        public static readonly Error AlreadyExists =
            new(
                "Vendor.AlreadyExists",
                "You already have a vendor account.",
                409
            );

        public static readonly Error InvalidBusinessName =
            new(
                "Vendor.InvalidBusinessName",
                "Business name is required.",
                400
            );

        public static readonly Error InvalidAddress =
            new(
                "Vendor.InvalidAddress",
                "Vendor address is required.",
                400
            );

        public static readonly Error GovernorateNotFound =
            new(
                "Vendor.GovernorateNotFound",
                "Governorate was not found.",
                404
            );

        public static readonly Error CategoryNotFound =
            new(
                "Vendor.CategoryNotFound",
                "One or more categories were not found.",
                404
            );

        public static readonly Error ZoneNotFound =
            new(
                "Vendor.ZoneNotFound",
                "One or more service zones were not found.",
                404
            );

        public static readonly Error CategoriesNotAllowed =
            new(
                "Vendor.CategoriesNotAllowed",
                "Categories are only available for material suppliers.",
                400
            );

        public static readonly Error CategoriesRequired =
            new(
                "Vendor.CategoriesRequired",
                "Material suppliers must select at least one category.",
                400
            );

        public static readonly Error ServiceZonesRequired =
            new(
                "Vendor.ServiceZonesRequired",
                "At least one service zone is required.",
                400
            );

        public static readonly Error InvalidStatus =
            new(
                "Vendor.InvalidStatus",
                "Vendor cannot perform this action in its current status.",
                400
            );

        public static readonly Error DocumentRequirementNotFound =
            new(
                "Vendor.DocumentRequirementNotFound",
                "Document requirement was not found.",
                404
            );

        public static readonly Error DocumentNotFound =
            new(
                "Vendor.DocumentNotFound",
                "Vendor document was not found.",
                404
            );

        public static readonly Error MissingRequiredDocuments =
            new(
                "Vendor.MissingRequiredDocuments",
                "All required documents must be uploaded before submission.",
                400
            );

        public static readonly Error RejectedRequiredDocuments =
            new(
                "Vendor.RejectedRequiredDocuments",
                "Rejected required documents must be replaced before resubmission.",
                400
            );

        public static readonly Error RequiredDocumentsNotApproved =
            new(
                "Vendor.RequiredDocumentsNotApproved",
                "All required documents must be approved before approving the vendor.",
                400
            );

        public static readonly Error InvalidFile =
            new(
                "Vendor.InvalidFile",
                "The uploaded document is invalid or unsupported.",
                400
            );

        public static readonly Error FileTooLarge =
            new(
                "Vendor.FileTooLarge",
                "The uploaded document exceeds the maximum allowed size.",
                400
            );

        public static readonly Error ReasonRequired =
            new(
                "Vendor.ReasonRequired",
                "A reason is required.",
                400
            );

        public static readonly Error ReviewNotesRequired =
            new(
                "Vendor.ReviewNotesRequired",
                "Review notes are required.",
                400
            );
    }
}