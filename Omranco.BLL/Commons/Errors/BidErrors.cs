namespace Omranco.BLL.Commons.Errors
{

    public static class BidErrors
    {
        public static readonly Error VendorNotFound =
            new(
                "Bid.VendorNotFound",
                "Vendor account not found.",
                404
            );

        public static readonly Error RfqNotFound =
            new(
                "Bid.RfqNotFound",
                "RFQ not found.",
                404
            );

        public static readonly Error NotInvited =
            new(
                "Bid.NotInvited",
                "Vendor is not invited to this RFQ.",
                400
            );

        public static readonly Error RfqNotOpen =
            new(
                "Bid.RfqNotOpen",
                "RFQ is not accepting bids.",
                400
            );

        public static readonly Error DeadlinePassed =
            new(
                "Bid.DeadlinePassed",
                "RFQ deadline has passed.",
                404
            );

        public static readonly Error AlreadySubmitted =
            new(
                "Bid.AlreadySubmitted",
                "You already submitted a bid for this RFQ.",
                400
            );

        public static readonly Error InvalidItems =
            new(
                "Bid.InvalidItems",
                "All RFQ items must be priced.",
                401
            );

        public static readonly Error InvalidPrice =
            new(
                "Bid.InvalidPrice",
                "Unit price must be greater than zero.",
                401
            );

        public static readonly Error InvalidValidity =
            new(
                "Bid.InvalidValidity",
                "Bid validity date must be in the future.",
                401
            );

        public static readonly Error AccessDenied =
    new(
        "Bid.AccessDenied",
        "You do not have access to this RFQ.",
        400
    );

        public static readonly Error NotFound =
            new(
                "Bid.NotFound",
                "Bid not found.",
                404
            );

        public static readonly Error CannotAccept =
            new(
                "Bid.CannotAccept",
                "This bid cannot be accepted.",
                400
            );

        public static readonly Error BidExpired =
            new(
                "Bid.Expired",
                "This bid has expired.",
                400
            );

        public static readonly Error RfqAlreadyAwarded =
            new(
                "Bid.RfqAlreadyAwarded",
                "A bid has already been accepted for this RFQ.",
                400
            );
    }
}

