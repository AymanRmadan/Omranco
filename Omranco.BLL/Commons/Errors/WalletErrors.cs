namespace Omranco.BLL.Commons.Errors
{
    public static class WalletErrors
    {
        public static readonly Error OrderNotFound =
            new(
                "Wallet.OrderNotFound",
                "Order not found.",
                404
            );

        public static readonly Error AlreadyCredited =
            new(
                "Wallet.AlreadyCredited",
                "This order has already been credited to the vendor wallet.",
                400
            );

        public static readonly Error InvalidNetAmount =
            new(
                "Wallet.InvalidNetAmount",
                "Vendor net amount must be greater than zero.",
                400
            );
        public static readonly Error WalletNotFound =
    new(
        "Wallet.NotFound",
        "Vendor wallet not found.",
        404
    );
    }
}