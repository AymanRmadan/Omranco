namespace Omranco.BLL.DTOS.Ratings.Requests;

public sealed record CreateVendorRatingRequest(
    int Stars,
    string? Comment
);