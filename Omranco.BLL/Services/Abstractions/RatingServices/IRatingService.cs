using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Ratings.Requests;

namespace Omranco.BLL.Services.Abstractions.RatingServices
{
    public interface IRatingService
    {
        Task<Result<int>> RateVendorAsync(int orderId, int currentUserId, SubmitVendorRatingRequest request);
    }
}