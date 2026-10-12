using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Ratings.Requests;
using Omranco.BLL.DTOS.Ratings.Responses;

namespace Omranco.BLL.Services.Abstractions.RatingServices
{
    public interface IVendorRatingService
    {
        Task<Result<VendorRatingResponse>>
            CreateAsync(
                int orderId,
                int currentUserId,
                CreateVendorRatingRequest request);

        Task<Result<VendorRatingResponse>>
            GetOrderRatingAsync(
                int orderId,
                int currentUserId);

        Task<Result<VendorRatingsResponse>>
            GetVendorRatingsAsync(
                int vendorId);

        Task<Result<VendorRatingsResponse>>
            GetMyVendorRatingsAsync(
                int currentUserId);
    }
}