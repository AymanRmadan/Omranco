namespace Omranco.BLL.DTOS.Ratings.Responses;

public sealed record VendorRatingResponse(
    int Id,
    int OrderId,
    int VendorId,
    int Stars,
    string? Comment,
    DateTime CreatedAt
);