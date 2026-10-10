namespace Omranco.BLL.Commons.Errors
{
    public static class CommissionErrors
    {
        public static readonly Error NoActiveRule =
            new(
                "Commission.NoActiveRule",
                "No active commission rule was found.",
                400
            );

        public static readonly Error InvalidAmount =
            new(
                "Commission.InvalidAmount",
                "Commission amount must be greater than zero.",
                400
            );

        public static readonly Error InvalidPercentage =
            new(
                "Commission.InvalidPercentage",
                "Commission percentage must be between 0 and 100.",
                400
            );
    }
}