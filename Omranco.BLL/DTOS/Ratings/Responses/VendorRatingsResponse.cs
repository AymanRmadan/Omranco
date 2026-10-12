namespace Omranco.BLL.DTOS.Ratings.Responses;

public sealed record VendorRatingsResponse(
    int VendorId,
    decimal RatingAverage,
    int RatingsCount,
    List<VendorRatingListItemResponse> Ratings
);