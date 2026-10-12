namespace Omranco.BLL.Commons.Errors
{
    public static class VendorRatingErrors
    {
        public static readonly Error OrderNotFound =
            new(
                "VendorRating.OrderNotFound",
                "Order not found.",
                404
            );

        public static readonly Error VendorNotFound =
            new(
                "VendorRating.VendorNotFound",
                "Vendor not found.",
                404
            );

        public static readonly Error AccessDenied =
            new(
                "VendorRating.AccessDenied",
                "You do not have access to rate this order.",
                403
            );

        public static readonly Error OrderNotDelivered =
            new(
                "VendorRating.OrderNotDelivered",
                "The order can only be rated after delivery.",
                400
            );

        public static readonly Error AlreadyRated =
            new(
                "VendorRating.AlreadyRated",
                "This order has already been rated.",
                400
            );

        public static readonly Error InvalidStars =
            new(
                "VendorRating.InvalidStars",
                "Rating must be between 1 and 5 stars.",
                400
            );

        public static readonly Error RatingNotFound =
    new(
        "VendorRating.NotFound",
        "Rating not found.",
        404
    );
    }
}