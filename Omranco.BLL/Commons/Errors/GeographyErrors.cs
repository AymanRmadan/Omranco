namespace Omranco.BLL.Commons.Errors
{
    public static class GeographyErrors
    {
        public static readonly Error GovernorateNotFound =
            new(
                "Geography.GovernorateNotFound",
                "Governorate not found.",
                404
            );

        public static readonly Error ZoneNotFound =
            new(
                "Geography.ZoneNotFound",
                "Zone not found.",
                404
            );

        public static readonly Error InvalidName =
            new(
                "Geography.InvalidName",
                "Arabic name is required.",
                400
            );

        public static readonly Error GovernorateAlreadyExists =
            new(
                "Geography.GovernorateAlreadyExists",
                "A governorate with the same Arabic name already exists.",
                409
            );

        public static readonly Error ZoneAlreadyExists =
            new(
                "Geography.ZoneAlreadyExists",
                "A zone with the same Arabic name already exists in this governorate.",
                409
            );
    }
}