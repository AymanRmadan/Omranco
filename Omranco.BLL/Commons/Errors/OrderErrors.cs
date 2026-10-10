namespace Omranco.BLL.Commons.Errors
{
    public static class OrderErrors
    {
        public static readonly Error NotFound =
            new(
                "Order.NotFound",
                "Order not found.",
                404
            );

        public static readonly Error AccessDenied =
            new(
                "Order.AccessDenied",
                "You do not have access to this order.",
                403
            );

        public static readonly Error InvalidStatusTransition =
            new(
                "Order.InvalidStatusTransition",
                "Invalid order status transition.",
                401
            );

        public static readonly Error VendorNotFound =
            new(
                "Order.VendorNotFound",
                "Vendor account not found.",
                404
            );
    }
}