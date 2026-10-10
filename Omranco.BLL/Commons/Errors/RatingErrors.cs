namespace Omranco.BLL.Commons.Errors
{
    public static class RatingErrors
    {
        public static readonly Error OrderNotFound =
            new(
                "Rating.OrderNotFound",
                "Order not found.",
                404
            );

        public static readonly Error AccessDenied =
            new(
                "Rating.AccessDenied",
                "You do not have access to this order.",
                403
            );

        public static readonly Error OrderNotDelivered =
            new(
                "Rating.OrderNotDelivered",
                "The order must be delivered before it can be rated.",
                400
            );

        public static readonly Error AlreadyRated =
            new(
                "Rating.AlreadyRated",
                "This order has already been rated.",
                400
            );

        public static readonly Error InvalidStars =
            new(
                "Rating.InvalidStars",
                "Rating must be between 1 and 5 stars.",
                400
            );

        public static readonly Error VendorNotFound =
            new(
                "Rating.VendorNotFound",
                "Vendor not found.",
                404
            );
    }
}