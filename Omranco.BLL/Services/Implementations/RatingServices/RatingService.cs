using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Ratings.Requests;
using Omranco.BLL.Services.Abstractions.RatingServices;
using Omranco.Domain.Entities.Ratings;
using Omranco.Domain.Enums;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.RatingServices
{
    public sealed class RatingService : IRatingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RatingService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Result<int>> RateVendorAsync(
            int orderId,
            int currentUserId,
            SubmitVendorRatingRequest request)
        {
            // ========================================
            // Validate Stars
            // ========================================

            if (request.Stars is < 1 or > 5)
            {
                return Result.Failure<int>(
                    RatingErrors.InvalidStars
                );
            }


            // ========================================
            // Get Order
            // ========================================

            var order =
                await _unitOfWork.Orders
                    .FirstOrDefaultAsync(
                        x => x.Id == orderId,
                        x => x.Project
                    );


            if (order is null)
            {
                return Result.Failure<int>(
                    RatingErrors.OrderNotFound
                );
            }


            // ========================================
            // Must be Delivered
            // ========================================

            if (order.Status !=
                OrderStatus.Delivered)
            {
                return Result.Failure<int>(
                    RatingErrors.OrderNotDelivered
                );
            }


            // ========================================
            // Customer Account User
            // ========================================

            var accountUser =
                await _unitOfWork
                    .CustomerAccountUsers
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId ==
                                currentUserId
                            &&
                            x.CustomerAccountId ==
                                order.Project
                                    .CustomerAccountId
                            &&
                            x.IsActive
                    );


            if (accountUser is null)
            {
                return Result.Failure<int>(
                    RatingErrors.AccessDenied
                );
            }


            // ========================================
            // Project Access
            // ========================================

            var hasProjectAccess =
                accountUser.Role ==
                CustomerAccountUserRole.Owner;


            if (!hasProjectAccess)
            {
                hasProjectAccess =
                    await _unitOfWork
                        .ProjectMembers
                        .AnyAsync(
                            x =>
                                x.ProjectId ==
                                    order.ProjectId
                                &&
                                x.CustomerAccountUserId ==
                                    accountUser.Id
                                &&
                                x.IsActive
                        );
            }


            if (!hasProjectAccess)
            {
                return Result.Failure<int>(
                    RatingErrors.AccessDenied
                );
            }


            // ========================================
            // One Rating Per Order
            // ========================================

            var alreadyRated =
                await _unitOfWork
                    .VendorRatings
                    .AnyAsync(
                        x =>
                            x.OrderId ==
                            order.Id
                    );


            if (alreadyRated)
            {
                return Result.Failure<int>(
                    RatingErrors.AlreadyRated
                );
            }


            // ========================================
            // Vendor
            // ========================================

            var trackedVendor =
                await _unitOfWork.Vendors
                    .FindAsync(
                        order.VendorId
                    );


            if (trackedVendor is null)
            {
                return Result.Failure<int>(
                    RatingErrors.VendorNotFound
                );
            }


            // ========================================
            // Create Rating
            // ========================================

            var rating =
                new VendorRating
                {
                    OrderId =
                        order.Id,

                    VendorId =
                        order.VendorId,

                    CustomerAccountUserId =
                        accountUser.Id,

                    Stars =
                        request.Stars,

                    Comment =
                        request.Comment,

                    CreatedAt =
                        DateTime.UtcNow
                };


            await _unitOfWork
                .VendorRatings
                .InsertAsync(
                    rating
                );


            // ========================================
            // Update Vendor Rating Aggregate
            // ========================================

            var currentTotal =
                trackedVendor.RatingAverage *
                trackedVendor.RatingsCount;


            var newRatingsCount =
                trackedVendor.RatingsCount + 1;


            var newAverage =
                (currentTotal + request.Stars)
                /
                newRatingsCount;


            trackedVendor.RatingsCount =
                newRatingsCount;


            trackedVendor.RatingAverage =
                Math.Round(
                    newAverage,
                    2
                );


            // ========================================
            // Save
            // ========================================

            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                rating.Id
            );
        }
    }
}