namespace Omranco.BLL.Commons.Errors
{
    public static class CatalogErrors
    {
        public static readonly Error CategoryNotFound =
            new(
                "Catalog.CategoryNotFound",
                "Category not found.",
                404
            );

        public static readonly Error CategoryAlreadyExists =
            new(
                "Catalog.CategoryAlreadyExists",
                "A category with the same name already exists under this parent.",
                409
            );

        public static readonly Error InvalidCategoryParent =
            new(
                "Catalog.InvalidCategoryParent",
                "Invalid parent category.",
                400
            );

        public static readonly Error CategoryCycle =
            new(
                "Catalog.CategoryCycle",
                "A category cannot be moved under itself or one of its descendants.",
                400
            );

        public static readonly Error UnitNotFound =
            new(
                "Catalog.UnitNotFound",
                "Unit not found.",
                404
            );

        public static readonly Error UnitAlreadyExists =
            new(
                "Catalog.UnitAlreadyExists",
                "A unit with the same name already exists.",
                409
            );

        public static readonly Error ProductNotFound =
            new(
                "Catalog.ProductNotFound",
                "Product not found.",
                404
            );

        public static readonly Error ProductAlreadyExists =
            new(
                "Catalog.ProductAlreadyExists",
                "A product with the same name already exists in this category.",
                409
            );

        public static readonly Error ProductUnitsRequired =
            new(
                "Catalog.ProductUnitsRequired",
                "At least one unit must be assigned to the product.",
                400
            );

        public static readonly Error ExactlyOneDefaultUnitRequired =
            new(
                "Catalog.ExactlyOneDefaultUnitRequired",
                "Exactly one product unit must be marked as default.",
                400
            );

        public static readonly Error AttributeNotFound =
            new(
                "Catalog.AttributeNotFound",
                "Product attribute not found.",
                404
            );

        public static readonly Error AttributeAlreadyExists =
            new(
                "Catalog.AttributeAlreadyExists",
                "An attribute with the same name already exists for this product.",
                409
            );

        public static readonly Error OptionNotFound =
            new(
                "Catalog.OptionNotFound",
                "Product attribute option not found.",
                404
            );

        public static readonly Error OptionAlreadyExists =
            new(
                "Catalog.OptionAlreadyExists",
                "This option already exists for the attribute.",
                409
            );

        public static readonly Error VariantNotFound =
            new(
                "Catalog.VariantNotFound",
                "Product variant not found.",
                404
            );

        public static readonly Error InvalidVariantValues =
            new(
                "Catalog.InvalidVariantValues",
                "Variant attribute values are invalid or incomplete.",
                400
            );

        public static readonly Error SkuAlreadyExists =
            new(
                "Catalog.SkuAlreadyExists",
                "Another product variant already uses this SKU.",
                409
            );

        public static readonly Error InvalidName =
            new(
                "Catalog.InvalidName",
                "Name is required.",
                400
            );
    }
}