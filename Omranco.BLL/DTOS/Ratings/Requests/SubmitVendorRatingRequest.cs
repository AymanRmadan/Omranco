namespace Omranco.BLL.DTOS.Ratings.Requests
{
    public sealed record SubmitVendorRatingRequest(
        int Stars,
        string? Comment
    );
}