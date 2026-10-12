using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Ratings.Requests;
using Omranco.BLL.DTOS.Ratings.Responses;
using Omranco.BLL.Services.Abstractions.RatingServices;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Enums;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.RatingServices
{
    public sealed class VendorRatingService
        : IVendorRatingService
    {
        private readonly IUnitOfWork _unitOfWork;


        public VendorRatingService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork =
                unitOfWork;
        }


        // =====================================================
        // Customer - Create Rating
        // =====================================================

        public async Task<Result<VendorRatingResponse>>
            CreateAsync(
                int orderId,
                int currentUserId,
                CreateVendorRatingRequest request)
        {
            // ==========================================
            // Validate stars
            // ==========================================

            if (
                request.Stars < 1
                ||
                request.Stars > 5
            )
            {
                return Result
                    .Failure<VendorRatingResponse>(
                        VendorRatingErrors.InvalidStars
                    );
            }


            // ==========================================
            // Order
            // ==========================================

            var order =
                await _unitOfWork
                    .Orders
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            orderId,

                        x => x.Project
                    );


            if (order is null)
            {
                return Result
                    .Failure<VendorRatingResponse>(
                        VendorRatingErrors
                            .OrderNotFound
                    );
            }


            // ==========================================
            // Customer access
            // ==========================================

            var hasAccess =
                await HasCustomerAccessAsync(
                    order,
                    currentUserId
                );


            if (!hasAccess)
            {
                return Result
                    .Failure<VendorRatingResponse>(
                        VendorRatingErrors
                            .AccessDenied
                    );
            }


            // ==========================================
            // Delivered only
            // ==========================================

            if (
                order.Status !=
                OrderStatus.Delivered
            )
            {
                return Result
                    .Failure<VendorRatingResponse>(
                        VendorRatingErrors
                            .OrderNotDelivered
                    );
            }


            // ==========================================
            // One rating per Order
            // ==========================================

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
                return Result
                    .Failure<VendorRatingResponse>(
                        VendorRatingErrors
                            .AlreadyRated
                    );
            }


            // ==========================================
            // Vendor tracked
            // ==========================================

            var vendor =
                await _unitOfWork
                    .Vendors
                    .FindAsync(
                        order.VendorId
                    );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorRatingResponse>(
                        VendorRatingErrors
                            .VendorNotFound
                    );
            }


            var now =
                DateTime.UtcNow;


            var rating =
                new VendorRating
                {
                    OrderId =
                        order.Id,

                    VendorId =
                        order.VendorId,

                    RatedByUserId =
                        currentUserId,

                    Stars =
                        request.Stars,

                    Comment =
                        Normalize(
                            request.Comment
                        ),

                    CreatedAt =
                        now
                };


            await _unitOfWork
                .VendorRatings
                .InsertAsync(
                    rating
                );


            // ==========================================
            // Update Vendor Aggregate
            // ==========================================

            var oldRatingsCount =
                vendor.RatingsCount;


            var oldRatingAverage =
                vendor.RatingAverage;


            var newRatingsCount =
                oldRatingsCount + 1;


            var newRatingAverage =
                decimal.Round(
                    (
                        (
                            oldRatingAverage *
                            oldRatingsCount
                        )
                        +
                        request.Stars
                    )
                    /
                    newRatingsCount,
                    2,
                    MidpointRounding.AwayFromZero
                );


            vendor.RatingsCount =
                newRatingsCount;

            vendor.RatingAverage =
                newRatingAverage;


            // ==========================================
            // One Save
            // ==========================================

            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                Map(rating)
            );
        }


        // =====================================================
        // Customer - Get Order Rating
        // =====================================================

        public async Task<Result<VendorRatingResponse>>
            GetOrderRatingAsync(
                int orderId,
                int currentUserId)
        {
            var order =
                await _unitOfWork
                    .Orders
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            orderId,

                        x => x.Project
                    );


            if (order is null)
            {
                return Result
                    .Failure<VendorRatingResponse>(
                        VendorRatingErrors
                            .OrderNotFound
                    );
            }


            var hasAccess =
                await HasCustomerAccessAsync(
                    order,
                    currentUserId
                );


            if (!hasAccess)
            {
                return Result
                    .Failure<VendorRatingResponse>(
                        VendorRatingErrors
                            .AccessDenied
                    );
            }


            var rating =
                await _unitOfWork
                    .VendorRatings
                    .FirstOrDefaultAsync(
                        x =>
                            x.OrderId ==
                            order.Id
                    );


            if (rating is null)
            {
                return Result
                    .Failure<VendorRatingResponse>(
                        VendorRatingErrors.RatingNotFound
                    );
            }


            return Result.Success(
                Map(rating)
            );
        }


        // =====================================================
        // Vendor Ratings
        // =====================================================

        public async Task<Result<VendorRatingsResponse>>
            GetVendorRatingsAsync(
                int vendorId)
        {
            var vendor =
                await _unitOfWork
                    .Vendors
                    .GetByIdAsync(
                        vendorId
                    );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorRatingsResponse>(
                        VendorRatingErrors
                            .VendorNotFound
                    );
            }


            return await BuildVendorRatingsResponseAsync(
                vendor
            );
        }


        // =====================================================
        // Current Vendor Ratings
        // =====================================================

        public async Task<Result<VendorRatingsResponse>>
            GetMyVendorRatingsAsync(
                int currentUserId)
        {
            var vendor =
                await _unitOfWork
                    .Vendors
                    .FirstOrDefaultAsync(
                        x =>
                            x.OwnerUserId ==
                                currentUserId
                            &&
                            x.IsActive
                    );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorRatingsResponse>(
                        VendorRatingErrors
                            .VendorNotFound
                    );
            }


            return await BuildVendorRatingsResponseAsync(
                vendor
            );
        }


        // =====================================================
        // Helpers
        // =====================================================

        private async Task<bool>
            HasCustomerAccessAsync(
                Order order,
                int currentUserId)
        {
            var accountUser =
                await _unitOfWork
                    .CustomerAccountUsers
                    .FirstOrDefaultAsync(
                        x =>
                            x.CustomerAccountId ==
                                order.Project
                                    .CustomerAccountId
                            &&
                            x.UserId ==
                                currentUserId
                            &&
                            x.IsActive
                    );


            if (accountUser is null)
            {
                return false;
            }


            if (
                accountUser.Role ==
                CustomerAccountUserRole.Owner
            )
            {
                return true;
            }


            return await _unitOfWork
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


        private async Task<Result<VendorRatingsResponse>>
            BuildVendorRatingsResponseAsync(
                Vendor vendor)
        {
            var ratings =
                await _unitOfWork
                    .VendorRatings
                    .GetAllAsync(
                        filter:
                            x =>
                                x.VendorId ==
                                vendor.Id,

                        orderBy:
                            query =>
                                query.OrderByDescending(
                                    x => x.CreatedAt
                                )
                    );


            var response =
                new VendorRatingsResponse(
                    vendor.Id,
                    vendor.RatingAverage,
                    vendor.RatingsCount,

                    ratings
                        .Select(
                            rating =>
                                new VendorRatingListItemResponse(
                                    rating.Id,
                                    rating.OrderId,
                                    rating.Stars,
                                    rating.Comment,
                                    rating.CreatedAt
                                )
                        )
                        .ToList()
                );


            return Result.Success(
                response
            );
        }


        private static VendorRatingResponse Map(
            VendorRating rating)
        {
            return new VendorRatingResponse(
                rating.Id,
                rating.OrderId,
                rating.VendorId,
                rating.Stars,
                rating.Comment,
                rating.CreatedAt
            );
        }


        private static string? Normalize(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }
    }
}