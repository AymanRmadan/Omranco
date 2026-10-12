namespace Omranco.BLL.DTOS.Ratings.Responses;

public sealed record VendorRatingListItemResponse(
    int Id,
    int OrderId,
    int Stars,
    string? Comment,
    DateTime CreatedAt
);