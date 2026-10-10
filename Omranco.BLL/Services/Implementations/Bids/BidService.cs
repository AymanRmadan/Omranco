using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Bids.Requests;
using Omranco.BLL.DTOS.Bids.Responses;
using Omranco.BLL.Services.Abstractions.BidServices;
using Omranco.Domain.Entities.Bids;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Enums;
using Omranco.Domain.Enums.Bids;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Enums.Rfqs;
using Omranco.Domain.Enums.Vendors;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.BidServices
{
    public sealed class BidService : IBidService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BidService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Result<int>> SubmitAsync(int rfqId, int currentUserId, SubmitBidRequest request)
        {
            // ========================================
            // Get Vendor
            // ========================================

            var vendor =
                await _unitOfWork.Vendors
                    .FirstOrDefaultAsync(
                        x =>
                            x.OwnerUserId ==
                                currentUserId &&
                            x.IsActive &&
                            x.Status ==
                                VendorStatus.Approved
                    );


            if (vendor is null)
            {
                return Result.Failure<int>(
                    BidErrors.VendorNotFound
                );
            }


            // ========================================
            // Get RFQ
            // ========================================

            var rfq =
                await _unitOfWork.Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Items
                    );


            if (rfq is null)
            {
                return Result.Failure<int>(
                    BidErrors.RfqNotFound
                );
            }


            if (rfq.Status != RfqStatus.Open)
            {
                return Result.Failure<int>(
                    BidErrors.RfqNotOpen
                );
            }


            if (!rfq.DeadlineAt.HasValue ||
                rfq.DeadlineAt.Value <=
                    DateTime.UtcNow)
            {
                return Result.Failure<int>(
                    BidErrors.DeadlinePassed
                );
            }


            // ========================================
            // Check Invitation
            // ========================================

            var invitation =
                await _unitOfWork
                    .RfqInvitations
                    .FirstOrDefaultAsync(
                        x =>
                            x.RfqId == rfqId &&
                            x.VendorId == vendor.Id
                    );


            if (invitation is null)
            {
                return Result.Failure<int>(
                    BidErrors.NotInvited
                );
            }


            // ========================================
            // One Bid Per Vendor / RFQ
            // ========================================

            var alreadySubmitted =
                await _unitOfWork.Bids
                    .AnyAsync(
                        x =>
                            x.RfqId == rfqId &&
                            x.VendorId == vendor.Id
                    );


            if (alreadySubmitted)
            {
                return Result.Failure<int>(
                    BidErrors.AlreadySubmitted
                );
            }


            // ========================================
            // Bid Validity
            // ========================================

            if (request.ValidUntil <=
                DateTime.UtcNow)
            {
                return Result.Failure<int>(
                    BidErrors.InvalidValidity
                );
            }


            // ========================================
            // Validate Prices
            // ========================================

            if (request.Items.Any(
                    x => x.UnitPrice <= 0))
            {
                return Result.Failure<int>(
                    BidErrors.InvalidPrice
                );
            }


            // ========================================
            // Validate RFQ Items
            // ========================================

            var rfqItemIds =
                rfq.Items
                    .Select(x => x.Id)
                    .OrderBy(x => x)
                    .ToList();


            var submittedItemIds =
                request.Items
                    .Select(x => x.RfqItemId)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();


            if (rfqItemIds.Count > 0 &&
                !rfqItemIds.SequenceEqual(
                    submittedItemIds))
            {
                return Result.Failure<int>(
                    BidErrors.InvalidItems
                );
            }


            if (request.ShippingCost < 0 || request.LeadTimeDays < 0)
            {
                return Result.Failure<int>(
                    BidErrors.InvalidPrice
                );
            }
            // ========================================
            // Build Bid Items
            // ========================================

            var bidItems =
                new List<BidItem>();


            foreach (var requestItem
                     in request.Items)
            {
                var rfqItem =
                    rfq.Items.First(
                        x =>
                            x.Id ==
                            requestItem.RfqItemId
                    );


                var totalPrice =
                    rfqItem.Quantity *
                    requestItem.UnitPrice;


                bidItems.Add(
                    new BidItem
                    {
                        RfqItemId =
                            rfqItem.Id,

                        UnitPrice =
                            requestItem.UnitPrice,

                        TotalPrice =
                            totalPrice,

                        Notes =
                            requestItem.Notes
                    }
                );
            }


            // ========================================
            // Calculate Totals
            // ========================================

            decimal subtotal;

            if (rfq.Items.Count > 0)
            {
                subtotal = bidItems.Sum(x => x.TotalPrice);
            }
            else
            {
                if (!request.OverallPrice.HasValue || request.OverallPrice.Value <= 0)
                {
                    return Result.Failure<int>(BidErrors.InvalidPrice);
                }

                subtotal = request.OverallPrice.Value;
            }


            var shippingCost =
                request.ShippingCostType ==
                    ShippingCostType.Separate
                    ? request.ShippingCost
                    : 0;


            var totalAmount =
                subtotal + shippingCost;


            // ========================================
            // Create Bid
            // ========================================

            var bid =
                new Bid
                {
                    RfqId = rfq.Id,

                    VendorId = vendor.Id,

                    Status =
                        BidStatus.Submitted,

                    Subtotal = subtotal,

                    ShippingCostType =
                        request.ShippingCostType,

                    ShippingCost =
                        shippingCost,

                    TotalAmount =
                        totalAmount,

                    LeadTimeDays =
                        request.LeadTimeDays,

                    PaymentTerms =
                        request.PaymentTerms,

                    ValidUntil =
                        request.ValidUntil,

                    Notes =
                        request.Notes,

                    SubmittedAt =
                        DateTime.UtcNow,

                    Items = bidItems
                };


            await _unitOfWork.Bids
                .InsertAsync(bid);


            // ========================================
            // Update Invitation
            // ========================================

            var trackedInvitation =
                await _unitOfWork
                    .RfqInvitations
                    .FindAsync(
                        invitation.Id
                    );


            if (trackedInvitation is null)
            {
                return Result.Failure<int>(
                    BidErrors.NotInvited
                );
            }


            trackedInvitation.Status =
                RfqInvitationStatus.Responded;

            trackedInvitation.RespondedAt =
                DateTime.UtcNow;


            // ========================================
            // Save
            // ========================================

            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                bid.Id
            );
        }

        public async Task<Result<List<BidComparisonResponse>>> GetComparisonAsync(int rfqId, int currentUserId)
        {
            var rfq =
                await _unitOfWork.Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Project
                    );

            if (rfq is null)
            {
                return Result.Failure<List<BidComparisonResponse>>(
                    BidErrors.RfqNotFound
                );
            }


            // ========================================
            // Customer Access
            // ========================================

            var accountUser =
                await _unitOfWork.CustomerAccountUsers
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId == currentUserId &&
                            x.CustomerAccountId ==
                                rfq.Project.CustomerAccountId &&
                            x.IsActive
                    );

            if (accountUser is null)
            {
                return Result.Failure<List<BidComparisonResponse>>(
                    BidErrors.AccessDenied
                );
            }


            var hasAccess =
                accountUser.Role ==
                CustomerAccountUserRole.Owner;


            if (!hasAccess)
            {
                hasAccess =
                    await _unitOfWork.ProjectMembers
                        .AnyAsync(
                            x =>
                                x.ProjectId == rfq.ProjectId &&
                                x.CustomerAccountUserId ==
                                    accountUser.Id &&
                                x.IsActive
                        );
            }


            if (!hasAccess)
            {
                return Result.Failure<List<BidComparisonResponse>>(
                    BidErrors.AccessDenied
                );
            }


            // ========================================
            // Load Bids
            // ========================================

            var bids =
                await _unitOfWork.Bids
                    .GetAllAsync(
                        filter: x =>
                            x.RfqId == rfqId &&
                            x.Status != BidStatus.Withdrawn,

                        orderBy: query =>
                            query
                                .OrderBy(x => x.SubmittedAt)
                                .ThenBy(x => x.Id),

                        includes:
                        [
                            x => x.Vendor,
                    x => x.Items
                        ]
                    );


            /*
             * OfferNumber بيتحدد حسب ترتيب وصول العرض.
             *
             * وبعدين نقدر نرتب النتيجة بالسعر
             * بدون ما OfferNumber يتغير.
             */

            var responses = bids
                .Select((bid, index) =>
                    new BidComparisonResponse(
                        bid.Id,
                        index + 1,
                        bid.Vendor.RatingAverage,
                        bid.Vendor.RatingsCount,
                        bid.Subtotal,
                        bid.ShippingCostType.ToString(),
                        bid.ShippingCost,
                        bid.TotalAmount,
                        bid.LeadTimeDays,
                        bid.PaymentTerms,
                        bid.ValidUntil,
                        bid.ValidUntil <= DateTime.UtcNow,
                        bid.Notes,
                        bid.Items
                            .Select(item => new BidComparisonItemResponse(
                                item.RfqItemId,
                                item.UnitPrice,
                                item.TotalPrice,
                                item.Notes
                            ))
                            .ToList()
                    )
                )
                .OrderBy(x => x.TotalAmount)
                .ToList();



            return Result.Success(responses);
        }

        public async Task<Result<int>> AcceptAsync(int rfqId, int bidId, int currentUserId)
        {
            // ========================================
            // RFQ
            // ========================================

            var rfq =
                await _unitOfWork.Rfqs
                    .FirstOrDefaultAsync(
                        x => x.Id == rfqId,
                        x => x.Project,
                        x => x.Items
                    );


            if (rfq is null)
            {
                return Result.Failure<int>(
                    BidErrors.RfqNotFound
                );
            }


            if (rfq.Status == RfqStatus.Awarded)
            {
                return Result.Failure<int>(
                    BidErrors.RfqAlreadyAwarded
                );
            }


            if (rfq.Status != RfqStatus.Open &&
                rfq.Status != RfqStatus.Closed)
            {
                return Result.Failure<int>(
                    BidErrors.CannotAccept
                );
            }


            // ========================================
            // Customer Access
            // ========================================

            var accountUser =
                await _unitOfWork.CustomerAccountUsers
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId == currentUserId &&
                            x.CustomerAccountId ==
                                rfq.Project.CustomerAccountId &&
                            x.IsActive
                    );


            if (accountUser is null)
            {
                return Result.Failure<int>(
                    BidErrors.AccessDenied
                );
            }


            var hasAccess =
                accountUser.Role ==
                CustomerAccountUserRole.Owner;


            if (!hasAccess)
            {
                hasAccess =
                    await _unitOfWork.ProjectMembers
                        .AnyAsync(
                            x =>
                                x.ProjectId == rfq.ProjectId &&
                                x.CustomerAccountUserId ==
                                    accountUser.Id &&
                                x.IsActive
                        );
            }


            if (!hasAccess)
            {
                return Result.Failure<int>(
                    BidErrors.AccessDenied
                );
            }


            // ========================================
            // Selected Bid
            // ========================================

            var selectedBid =
                await _unitOfWork.Bids
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == bidId &&
                            x.RfqId == rfqId,
                        x => x.Items
                    );


            if (selectedBid is null)
            {
                return Result.Failure<int>(
                    BidErrors.NotFound
                );
            }


            if (selectedBid.Status !=
                BidStatus.Submitted)
            {
                return Result.Failure<int>(
                    BidErrors.CannotAccept
                );
            }


            if (selectedBid.ValidUntil <=
                DateTime.UtcNow)
            {
                return Result.Failure<int>(
                    BidErrors.BidExpired
                );
            }


            // ========================================
            // Load all bids
            // ========================================

            var allBids =
                await _unitOfWork.Bids
                    .GetAllAsync(
                        filter: x =>
                            x.RfqId == rfqId
                    );


            var now = DateTime.UtcNow;


            // ========================================
            // Update Bid Statuses
            // ========================================

            foreach (var bid in allBids)
            {
                if (bid.Status != BidStatus.Submitted)
                    continue;


                var trackedBid =
                    await _unitOfWork.Bids
                        .FindAsync(bid.Id);


                if (trackedBid is null)
                    continue;


                if (bid.Id == selectedBid.Id)
                {
                    trackedBid.Status =
                        BidStatus.Accepted;

                    trackedBid.AcceptedAt = now;
                }
                else
                {
                    trackedBid.Status =
                        BidStatus.Rejected;

                    trackedBid.RejectedAt = now;
                }
            }


            // ========================================
            // Award RFQ
            // ========================================

            var trackedRfq = await _unitOfWork.Rfqs.FindAsync(rfq.Id);


            if (trackedRfq is null)
            {
                return Result.Failure<int>(BidErrors.RfqNotFound);
            }


            trackedRfq.Status = RfqStatus.Awarded;
            trackedRfq.UpdatedAt = now;


            // ========================================
            // Build Order Items
            // ========================================

            var orderItems = new List<OrderItem>();


            foreach (var bidItem in selectedBid.Items)
            {
                var rfqItem =
                    rfq.Items.FirstOrDefault(x => x.Id == bidItem.RfqItemId);


                if (rfqItem is null)
                    continue;


                orderItems.Add(new OrderItem
                {
                    RfqItemId = rfqItem.Id,
                    ProductId = rfqItem.ProductId,
                    ProductVariantId = rfqItem.ProductVariantId,
                    UnitId = rfqItem.UnitId,
                    Quantity = rfqItem.Quantity,
                    UnitPrice = bidItem.UnitPrice,
                    TotalPrice = bidItem.TotalPrice
                }
                );
            }

            // ========================================
            // Create Order
            // ========================================

            var order = new Order
            {
                Number = GenerateOrderNumber(),
                RfqId = rfq.Id,
                BidId = selectedBid.Id,
                ProjectId = rfq.ProjectId,
                VendorId = selectedBid.VendorId,
                AcceptedByAccountUserId = accountUser.Id,
                Status = OrderStatus.Confirmed,
                Subtotal = selectedBid.Subtotal,
                ShippingCost = selectedBid.ShippingCost,
                TotalAmount = selectedBid.TotalAmount,
                CreatedAt = now,
                Items = orderItems,
                StatusHistory = [
                            new OrderStatusHistory
                            {
                                OldStatus = null,

                                NewStatus =
                                    OrderStatus.Confirmed,

                                ChangedByUserId =
                                    currentUserId,

                                ChangedAt =
                                    now
                            }
]
            };


            await _unitOfWork.Orders
                .InsertAsync(order);


            // ========================================
            // One SaveChanges
            // ========================================

            await _unitOfWork.SaveChangeAsync();

            return Result.Success(order.Id);
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}

