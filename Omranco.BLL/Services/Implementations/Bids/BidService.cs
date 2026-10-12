using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Bids.Requests;
using Omranco.BLL.DTOS.Bids.Responses;
using Omranco.BLL.Services.Abstractions.BidServices;
using Omranco.BLL.Services.Abstractions.CommissionServices;
using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Bids;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Entities.Payments;
using Omranco.Domain.Entities.Rfqs;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Enums;
using Omranco.Domain.Enums.Bids;
using Omranco.Domain.Enums.Orders;
using Omranco.Domain.Enums.Payments;
using Omranco.Domain.Enums.Rfqs;
using Omranco.Domain.Enums.Vendors;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.BidServices
{
    public sealed class BidService : IBidService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommissionService _commissionService;


        public BidService(
            IUnitOfWork unitOfWork,
            ICommissionService commissionService)
        {
            _unitOfWork =
                unitOfWork;

            _commissionService =
                commissionService;
        }


        // =====================================================
        // Vendor - Invitations
        // =====================================================

        public async Task<Result<List<VendorRfqInvitationResponse>>>
            GetMyInvitationsAsync(
                int currentUserId)
        {
            var vendor =
                await GetApprovedVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result
                    .Failure<List<VendorRfqInvitationResponse>>(
                        BidErrors.VendorNotFound
                    );
            }


            var invitations =
                await _unitOfWork
                    .RfqInvitations
                    .GetAllAsync(
                        filter:
                            x =>
                                x.VendorId ==
                                vendor.Id,

                        orderBy:
                            query =>
                                query.OrderByDescending(
                                    x => x.SentAt
                                ),

                        includes:
                        [
                            x => x.Rfq
                        ]
                    );


            var now =
                DateTime.UtcNow;


            var responses =
                new List<VendorRfqInvitationResponse>();


            foreach (var invitation in invitations)
            {
                var status =
                    invitation.Status;


                if (
                    (
                        status ==
                            RfqInvitationStatus.Sent
                        ||
                        status ==
                            RfqInvitationStatus.Viewed
                    )
                    &&
                    invitation.Rfq.DeadlineAt.HasValue
                    &&
                    invitation.Rfq.DeadlineAt.Value <= now
                )
                {
                    status =
                        RfqInvitationStatus.Expired;


                    var trackedInvitation =
                        await _unitOfWork
                            .RfqInvitations
                            .FindAsync(
                                invitation.Id
                            );


                    if (trackedInvitation is not null)
                    {
                        trackedInvitation.Status =
                            RfqInvitationStatus.Expired;
                    }
                }


                responses.Add(
                    new VendorRfqInvitationResponse(
                        invitation.Id,
                        invitation.RfqId,
                        invitation.Rfq.Number,
                        status,
                        invitation.Rfq.DeadlineAt,
                        invitation.SentAt,
                        invitation.ViewedAt,
                        invitation.RespondedAt,
                        invitation.DeclinedAt
                    )
                );
            }


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                responses
            );
        }


        public async Task<Result<VendorRfqInvitationDetailsResponse>>
            GetInvitationAsync(
                int rfqId,
                int currentUserId)
        {
            var vendor =
                await GetApprovedVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorRfqInvitationDetailsResponse>(
                        BidErrors.VendorNotFound
                    );
            }


            var invitation =
                await _unitOfWork
                    .RfqInvitations
                    .FirstOrDefaultAsync(
                        x =>
                            x.RfqId ==
                                rfqId
                            &&
                            x.VendorId ==
                                vendor.Id
                    );


            if (invitation is null)
            {
                return Result
                    .Failure<VendorRfqInvitationDetailsResponse>(
                        BidErrors.NotInvited
                    );
            }


            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            rfqId,

                        x => x.Items
                    );


            if (rfq is null)
            {
                return Result
                    .Failure<VendorRfqInvitationDetailsResponse>(
                        BidErrors.RfqNotFound
                    );
            }


            var now =
                DateTime.UtcNow;


            var effectiveStatus =
                invitation.Status;

            var viewedAt =
                invitation.ViewedAt;


            // ==========================================
            // Expired Invitation
            // ==========================================

            if (
                (
                    invitation.Status ==
                        RfqInvitationStatus.Sent
                    ||
                    invitation.Status ==
                        RfqInvitationStatus.Viewed
                )
                &&
                rfq.DeadlineAt.HasValue
                &&
                rfq.DeadlineAt.Value <= now
            )
            {
                var trackedInvitation =
                    await _unitOfWork
                        .RfqInvitations
                        .FindAsync(
                            invitation.Id
                        );


                if (trackedInvitation is not null)
                {
                    trackedInvitation.Status =
                        RfqInvitationStatus.Expired;

                    effectiveStatus =
                        RfqInvitationStatus.Expired;
                }


                await _unitOfWork
                    .SaveChangeAsync();
            }


            // ==========================================
            // Mark Viewed
            // ==========================================

            else if (
                invitation.Status ==
                RfqInvitationStatus.Sent)
            {
                var trackedInvitation =
                    await _unitOfWork
                        .RfqInvitations
                        .FindAsync(
                            invitation.Id
                        );


                if (trackedInvitation is not null)
                {
                    trackedInvitation.Status =
                        RfqInvitationStatus.Viewed;

                    trackedInvitation.ViewedAt =
                        now;

                    effectiveStatus =
                        RfqInvitationStatus.Viewed;

                    viewedAt =
                        now;


                    await _unitOfWork
                        .SaveChangeAsync();
                }
            }


            return Result.Success(
                new VendorRfqInvitationDetailsResponse(
                    invitation.Id,
                    rfq.Id,
                    rfq.Number,
                    effectiveStatus,
                    rfq.DeadlineAt,
                    rfq.DesiredDeliveryDate,
                    rfq.Notes,
                    invitation.SentAt,
                    viewedAt,
                    rfq.Items
                        .Select(
                            x =>
                                new VendorRfqItemResponse(
                                    x.Id,
                                    x.ProductId,
                                    x.ProductVariantId,
                                    x.UnitId,
                                    x.Quantity,
                                    x.Notes
                                )
                        )
                        .ToList()
                )
            );
        }


        public async Task<Result> DeclineInvitationAsync(
            int rfqId,
            int currentUserId)
        {
            var vendor =
                await GetApprovedVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result.Failure(
                    BidErrors.VendorNotFound
                );
            }


            var rfq =
                await _unitOfWork
                    .Rfqs
                    .GetByIdAsync(
                        rfqId
                    );


            if (rfq is null)
            {
                return Result.Failure(
                    BidErrors.RfqNotFound
                );
            }


            var invitation =
                await _unitOfWork
                    .RfqInvitations
                    .FirstOrDefaultAsync(
                        x =>
                            x.RfqId ==
                                rfqId
                            &&
                            x.VendorId ==
                                vendor.Id
                    );


            if (invitation is null)
            {
                return Result.Failure(
                    BidErrors.NotInvited
                );
            }


            if (
                invitation.Status !=
                    RfqInvitationStatus.Sent
                &&
                invitation.Status !=
                    RfqInvitationStatus.Viewed
            )
            {
                return Result.Failure(
                    BidErrors
                        .InvitationCannotBeDeclined
                );
            }


            if (
                !rfq.DeadlineAt.HasValue
                ||
                rfq.DeadlineAt.Value <=
                    DateTime.UtcNow
            )
            {
                var trackedExpiredInvitation =
                    await _unitOfWork
                        .RfqInvitations
                        .FindAsync(
                            invitation.Id
                        );


                if (trackedExpiredInvitation is not null)
                {
                    trackedExpiredInvitation.Status =
                        RfqInvitationStatus.Expired;


                    await _unitOfWork
                        .SaveChangeAsync();
                }


                return Result.Failure(
                    BidErrors.DeadlinePassed
                );
            }


            var trackedInvitation =
                await _unitOfWork
                    .RfqInvitations
                    .FindAsync(
                        invitation.Id
                    );


            if (trackedInvitation is null)
            {
                return Result.Failure(
                    BidErrors.NotInvited
                );
            }


            var now =
                DateTime.UtcNow;


            trackedInvitation.Status =
                RfqInvitationStatus.Declined;

            trackedInvitation.DeclinedAt =
                now;

            trackedInvitation.RespondedAt =
                now;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Vendor - Submit Bid
        // =====================================================

        public async Task<Result<int>> SubmitAsync(
            int rfqId,
            int currentUserId,
            SubmitBidRequest request)
        {
            var vendor =
                await GetApprovedVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result.Failure<int>(
                    BidErrors.VendorNotFound
                );
            }


            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            rfqId,

                        x => x.Items
                    );


            if (rfq is null)
            {
                return Result.Failure<int>(
                    BidErrors.RfqNotFound
                );
            }


            if (rfq.Status !=
                RfqStatus.Open)
            {
                return Result.Failure<int>(
                    BidErrors.RfqNotOpen
                );
            }


            if (
                !rfq.DeadlineAt.HasValue
                ||
                rfq.DeadlineAt.Value <=
                    DateTime.UtcNow
            )
            {
                return Result.Failure<int>(
                    BidErrors.DeadlinePassed
                );
            }


            // ==========================================
            // Invitation
            // ==========================================

            var invitation =
                await _unitOfWork
                    .RfqInvitations
                    .FirstOrDefaultAsync(
                        x =>
                            x.RfqId ==
                                rfqId
                            &&
                            x.VendorId ==
                                vendor.Id
                    );


            if (invitation is null)
            {
                return Result.Failure<int>(
                    BidErrors.NotInvited
                );
            }


            if (
                invitation.Status ==
                    RfqInvitationStatus.Declined
                ||
                invitation.Status ==
                    RfqInvitationStatus.Expired
            )
            {
                return Result.Failure<int>(
                    BidErrors.NotInvited
                );
            }


            // ==========================================
            // One Bid Per Vendor / RFQ
            // ==========================================

            var alreadySubmitted =
                await _unitOfWork
                    .Bids
                    .AnyAsync(
                        x =>
                            x.RfqId ==
                                rfqId
                            &&
                            x.VendorId ==
                                vendor.Id
                    );


            if (alreadySubmitted)
            {
                return Result.Failure<int>(
                    BidErrors.AlreadySubmitted
                );
            }


            // ==========================================
            // Calculate & Validate
            // ==========================================

            var bidInput =
                MapInput(request);


            var pricingResult =
                CalculateBid(
                    rfq,
                    bidInput
                );


            if (pricingResult.IsFailure)
            {
                return Result.Failure<int>(
                    pricingResult.Error
                );
            }


            var pricing =
                pricingResult.Value!;


            var now =
                DateTime.UtcNow;


            // ==========================================
            // Bid
            // ==========================================

            var bid =
                new Bid
                {
                    RfqId =
                        rfq.Id,

                    VendorId =
                        vendor.Id,

                    Status =
                        BidStatus.Submitted,

                    Subtotal =
                        pricing.Subtotal,

                    ShippingCostType =
                        request.ShippingCostType,

                    ShippingCost =
                        pricing.ShippingCost,

                    TotalAmount =
                        pricing.TotalAmount,

                    LeadTimeDays =
                        request.LeadTimeDays,

                    PaymentTerms =
                        Normalize(
                            request.PaymentTerms
                        ),

                    ValidUntil =
                        request.ValidUntil,

                    Notes =
                        Normalize(
                            request.Notes
                        ),

                    SubmittedAt =
                        now,

                    Items =
                        pricing.Items
                };


            await _unitOfWork
                .Bids
                .InsertAsync(
                    bid
                );


            // ==========================================
            // Invitation Responded
            // ==========================================

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
                now;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                bid.Id
            );
        }


        // =====================================================
        // Vendor - Get My Bid
        // =====================================================

        public async Task<Result<VendorBidResponse>>
            GetMyBidAsync(
                int rfqId,
                int currentUserId)
        {
            var vendor =
                await GetApprovedVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result
                    .Failure<VendorBidResponse>(
                        BidErrors.VendorNotFound
                    );
            }


            var bid =
                await _unitOfWork
                    .Bids
                    .FirstOrDefaultAsync(
                        x =>
                            x.RfqId ==
                                rfqId
                            &&
                            x.VendorId ==
                                vendor.Id,

                        x => x.Items
                    );


            if (bid is null)
            {
                return Result
                    .Failure<VendorBidResponse>(
                        BidErrors.NotFound
                    );
            }


            if (
                bid.Status ==
                    BidStatus.Submitted
                &&
                bid.ValidUntil <=
                    DateTime.UtcNow
            )
            {
                var trackedBid =
                    await _unitOfWork
                        .Bids
                        .FindAsync(
                            bid.Id
                        );


                if (trackedBid is not null)
                {
                    trackedBid.Status =
                        BidStatus.Expired;


                    await _unitOfWork
                        .SaveChangeAsync();
                }


                bid.Status =
                    BidStatus.Expired;
            }


            return Result.Success(
                MapVendorBid(
                    bid
                )
            );
        }


        // =====================================================
        // Vendor - Update Bid
        // =====================================================

        public async Task<Result> UpdateAsync(
            int rfqId,
            int currentUserId,
            UpdateBidRequest request)
        {
            var vendor =
                await GetApprovedVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result.Failure(
                    BidErrors.VendorNotFound
                );
            }


            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            rfqId,

                        x => x.Items
                    );


            if (rfq is null)
            {
                return Result.Failure(
                    BidErrors.RfqNotFound
                );
            }


            if (
                rfq.Status !=
                    RfqStatus.Open
                ||
                !rfq.DeadlineAt.HasValue
                ||
                rfq.DeadlineAt.Value <=
                    DateTime.UtcNow
            )
            {
                return Result.Failure(
                    BidErrors.CannotUpdate
                );
            }


            var bid =
                await _unitOfWork
                    .Bids
                    .FirstOrDefaultAsync(
                        x =>
                            x.RfqId ==
                                rfqId
                            &&
                            x.VendorId ==
                                vendor.Id,

                        x => x.Items
                    );


            if (bid is null)
            {
                return Result.Failure(
                    BidErrors.NotFound
                );
            }


            if (bid.Status !=
                BidStatus.Submitted)
            {
                return Result.Failure(
                    BidErrors.CannotUpdate
                );
            }


            var pricingResult =
                CalculateBid(
                    rfq,
                    MapInput(request)
                );


            if (pricingResult.IsFailure)
            {
                return pricingResult;
            }


            var pricing =
                pricingResult.Value!;


            // ==========================================
            // Delete old BidItems
            // ==========================================

            foreach (var oldItem in bid.Items)
            {
                var trackedOldItem =
                    await _unitOfWork
                        .BidItems
                        .FindAsync(
                            oldItem.Id
                        );


                if (trackedOldItem is not null)
                {
                    _unitOfWork
                        .BidItems
                        .Delete(
                            trackedOldItem
                        );
                }
            }


            // ==========================================
            // Tracked Bid
            // ==========================================

            var trackedBid =
                await _unitOfWork
                    .Bids
                    .FindAsync(
                        bid.Id
                    );


            if (trackedBid is null)
            {
                return Result.Failure(
                    BidErrors.NotFound
                );
            }


            trackedBid.Subtotal =
                pricing.Subtotal;

            trackedBid.ShippingCostType =
                request.ShippingCostType;

            trackedBid.ShippingCost =
                pricing.ShippingCost;

            trackedBid.TotalAmount =
                pricing.TotalAmount;

            trackedBid.LeadTimeDays =
                request.LeadTimeDays;

            trackedBid.PaymentTerms =
                Normalize(
                    request.PaymentTerms
                );

            trackedBid.ValidUntil =
                request.ValidUntil;

            trackedBid.Notes =
                Normalize(
                    request.Notes
                );

            trackedBid.UpdatedAt =
                DateTime.UtcNow;


            // ==========================================
            // New BidItems
            // ==========================================

            foreach (var item in pricing.Items)
            {
                item.BidId =
                    bid.Id;
            }


            if (pricing.Items.Count > 0)
            {
                await _unitOfWork
                    .BidItems
                    .InsertListAsync(
                        pricing.Items
                    );
            }


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Vendor - Withdraw
        // =====================================================

        public async Task<Result> WithdrawAsync(
            int rfqId,
            int currentUserId)
        {
            var vendor =
                await GetApprovedVendorAsync(
                    currentUserId
                );


            if (vendor is null)
            {
                return Result.Failure(
                    BidErrors.VendorNotFound
                );
            }


            var rfq =
                await _unitOfWork
                    .Rfqs
                    .GetByIdAsync(
                        rfqId
                    );


            if (rfq is null)
            {
                return Result.Failure(
                    BidErrors.RfqNotFound
                );
            }


            if (
                rfq.Status !=
                    RfqStatus.Open
                ||
                !rfq.DeadlineAt.HasValue
                ||
                rfq.DeadlineAt.Value <=
                    DateTime.UtcNow
            )
            {
                return Result.Failure(
                    BidErrors.CannotWithdraw
                );
            }


            var bid =
                await _unitOfWork
                    .Bids
                    .FirstOrDefaultAsync(
                        x =>
                            x.RfqId ==
                                rfqId
                            &&
                            x.VendorId ==
                                vendor.Id
                    );


            if (bid is null)
            {
                return Result.Failure(
                    BidErrors.NotFound
                );
            }


            if (bid.Status !=
                BidStatus.Submitted)
            {
                return Result.Failure(
                    BidErrors.CannotWithdraw
                );
            }


            var trackedBid =
                await _unitOfWork
                    .Bids
                    .FindAsync(
                        bid.Id
                    );


            if (trackedBid is null)
            {
                return Result.Failure(
                    BidErrors.NotFound
                );
            }


            var now =
                DateTime.UtcNow;


            trackedBid.Status =
                BidStatus.Withdrawn;

            trackedBid.WithdrawnAt =
                now;

            trackedBid.UpdatedAt =
                now;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Customer - Anonymous Comparison
        // =====================================================

        public async Task<Result<List<BidComparisonResponse>>>
            GetComparisonAsync(
                int rfqId,
                int currentUserId)
        {
            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            rfqId,

                        x => x.Project
                    );


            if (rfq is null)
            {
                return Result
                    .Failure<List<BidComparisonResponse>>(
                        BidErrors.RfqNotFound
                    );
            }


            var accountUser =
                await GetCustomerAccessAsync(
                    rfq,
                    currentUserId
                );


            if (accountUser is null)
            {
                return Result
                    .Failure<List<BidComparisonResponse>>(
                        BidErrors.AccessDenied
                    );
            }


            /*
             * مهم:
             * هنجيب كل الـBids حتى الـWithdrawn
             * عشان OfferNumber يفضل ثابت.
             */
            var bids =
                await _unitOfWork
                    .Bids
                    .GetAllAsync(
                        filter:
                            x =>
                                x.RfqId ==
                                rfqId,

                        orderBy:
                            query =>
                                query
                                    .OrderBy(
                                        x => x.SubmittedAt
                                    )
                                    .ThenBy(
                                        x => x.Id
                                    ),

                        includes:
                        [
                            x => x.Vendor,
                            x => x.Items
                        ]
                    );


            var now =
                DateTime.UtcNow;


            // ==========================================
            // Expire old submitted bids
            // ==========================================

            var hasExpiredChanges =
                false;


            foreach (var bid in bids)
            {
                if (
                    bid.Status ==
                        BidStatus.Submitted
                    &&
                    bid.ValidUntil <=
                        now
                )
                {
                    var trackedBid =
                        await _unitOfWork
                            .Bids
                            .FindAsync(
                                bid.Id
                            );


                    if (trackedBid is not null)
                    {
                        trackedBid.Status =
                            BidStatus.Expired;

                        hasExpiredChanges =
                            true;
                    }
                }
            }


            if (hasExpiredChanges)
            {
                await _unitOfWork
                    .SaveChangeAsync();
            }


            // ==========================================
            // Stable Offer Numbers
            // ==========================================

            var offerNumbers =
                bids
                    .Select(
                        (bid, index) =>
                            new
                            {
                                bid.Id,
                                OfferNumber =
                                    index + 1
                            }
                    )
                    .ToDictionary(
                        x => x.Id,
                        x => x.OfferNumber
                    );


            var responses =
                bids
                    .Where(
                        x =>
                            x.Status !=
                            BidStatus.Withdrawn
                    )
                    .Select(
                        bid =>
                            new BidComparisonResponse(
                                bid.Id,
                                offerNumbers[bid.Id],
                                bid.Vendor.RatingAverage,
                                bid.Vendor.RatingsCount,
                                bid.Subtotal,
                                bid.ShippingCostType
                                    .ToString(),
                                bid.ShippingCost,
                                bid.TotalAmount,
                                bid.LeadTimeDays,
                                bid.PaymentTerms,
                                bid.ValidUntil,
                                bid.ValidUntil <= now,
                                bid.Notes,
                                bid.Items
                                    .Select(
                                        item =>
                                            new BidComparisonItemResponse(
                                                item.RfqItemId,
                                                item.UnitPrice,
                                                item.TotalPrice,
                                                item.Notes
                                            )
                                    )
                                    .ToList()
                            )
                    )
                    .OrderBy(
                        x => x.TotalAmount
                    )
                    .ThenBy(
                        x => x.OfferNumber
                    )
                    .ToList();


            return Result.Success(
                responses
            );
        }


        // =====================================================
        // Customer - Accept Bid
        // =====================================================

        public async Task<Result<int>> AcceptAsync(
            int rfqId,
            int bidId,
            int currentUserId)
        {
            var rfq =
                await _unitOfWork
                    .Rfqs
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            rfqId,

                        x => x.Project,
                        x => x.Items
                    );


            if (rfq is null)
            {
                return Result.Failure<int>(
                    BidErrors.RfqNotFound
                );
            }


            // ==========================================
            // Idempotency if same bid was already accepted
            // ==========================================

            if (rfq.Status ==
                RfqStatus.Awarded)
            {
                var existingOrder =
                    await _unitOfWork
                        .Orders
                        .FirstOrDefaultAsync(
                            x =>
                                x.RfqId ==
                                rfqId
                        );


                if (
                    existingOrder is not null
                    &&
                    existingOrder.BidId ==
                        bidId
                )
                {
                    return Result.Success(
                        existingOrder.Id
                    );
                }


                return Result.Failure<int>(
                    BidErrors.RfqAlreadyAwarded
                );
            }


            if (
                rfq.Status !=
                    RfqStatus.Open
                &&
                rfq.Status !=
                    RfqStatus.Closed
            )
            {
                return Result.Failure<int>(
                    BidErrors.CannotAccept
                );
            }


            // ==========================================
            // Customer Access
            // ==========================================

            var accountUser =
                await GetCustomerAccessAsync(
                    rfq,
                    currentUserId
                );


            if (accountUser is null)
            {
                return Result.Failure<int>(
                    BidErrors.AccessDenied
                );
            }


            // ==========================================
            // Selected Bid
            // ==========================================

            var selectedBid =
                await _unitOfWork
                    .Bids
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                                bidId
                            &&
                            x.RfqId ==
                                rfqId,

                        x => x.Items
                    );


            if (selectedBid is null)
            {
                return Result.Failure<int>(
                    BidErrors.NotFound
                );
            }


            if (selectedBid.Status ==
                BidStatus.Expired)
            {
                return Result.Failure<int>(
                    BidErrors.BidExpired
                );
            }


            if (selectedBid.Status !=
                BidStatus.Submitted)
            {
                return Result.Failure<int>(
                    BidErrors.CannotAccept
                );
            }


            if (
                selectedBid.ValidUntil <=
                DateTime.UtcNow
            )
            {
                var expiredBid =
                    await _unitOfWork
                        .Bids
                        .FindAsync(
                            selectedBid.Id
                        );


                if (expiredBid is not null)
                {
                    expiredBid.Status =
                        BidStatus.Expired;


                    await _unitOfWork
                        .SaveChangeAsync();
                }


                return Result.Failure<int>(
                    BidErrors.BidExpired
                );
            }


            // ==========================================
            // Validate order items consistency
            // ==========================================

            if (
                rfq.Items.Count > 0
                &&
                selectedBid.Items.Count !=
                    rfq.Items.Count
            )
            {
                return Result.Failure<int>(
                    BidErrors.InvalidItems
                );
            }


            // ==========================================
            // Commission before changing states
            // ==========================================

            var commissionResult =
                await _commissionService
                    .CalculateAsync(
                        selectedBid.TotalAmount
                    );


            if (commissionResult.IsFailure)
            {
                return Result.Failure<int>(
                    commissionResult.Error
                );
            }


            var commission =
                commissionResult.Value!;


            var allBids =
                await _unitOfWork
                    .Bids
                    .GetAllAsync(
                        filter:
                            x =>
                                x.RfqId ==
                                rfqId
                    );


            var now =
                DateTime.UtcNow;


            // ==========================================
            // Accept selected / reject others
            // ==========================================

            foreach (var bid in allBids)
            {
                if (bid.Status !=
                    BidStatus.Submitted)
                {
                    continue;
                }


                var trackedBid =
                    await _unitOfWork
                        .Bids
                        .FindAsync(
                            bid.Id
                        );


                if (trackedBid is null)
                {
                    continue;
                }


                if (bid.Id ==
                    selectedBid.Id)
                {
                    trackedBid.Status =
                        BidStatus.Accepted;

                    trackedBid.AcceptedAt =
                        now;
                }
                else
                {
                    trackedBid.Status =
                        BidStatus.Rejected;

                    trackedBid.RejectedAt =
                        now;
                }
            }


            // ==========================================
            // Award RFQ
            // ==========================================

            var trackedRfq =
                await _unitOfWork
                    .Rfqs
                    .FindAsync(
                        rfq.Id
                    );


            if (trackedRfq is null)
            {
                return Result.Failure<int>(
                    BidErrors.RfqNotFound
                );
            }


            trackedRfq.Status =
                RfqStatus.Awarded;

            trackedRfq.UpdatedAt =
                now;


            // ==========================================
            // Expire unanswered invitations
            // ==========================================

            var invitations =
                await _unitOfWork
                    .RfqInvitations
                    .GetAllAsync(
                        filter:
                            x =>
                                x.RfqId ==
                                    rfqId
                                &&
                                (
                                    x.Status ==
                                        RfqInvitationStatus.Sent
                                    ||
                                    x.Status ==
                                        RfqInvitationStatus.Viewed
                                )
                    );


            foreach (var invitation in invitations)
            {
                var trackedInvitation =
                    await _unitOfWork
                        .RfqInvitations
                        .FindAsync(
                            invitation.Id
                        );


                if (trackedInvitation is not null)
                {
                    trackedInvitation.Status =
                        RfqInvitationStatus.Expired;
                }
            }


            // ==========================================
            // Build Order Items
            // ==========================================

            var orderItems =
                new List<OrderItem>();


            foreach (var bidItem in selectedBid.Items)
            {
                var rfqItem =
                    rfq.Items
                        .FirstOrDefault(
                            x =>
                                x.Id ==
                                bidItem.RfqItemId
                        );


                if (rfqItem is null)
                {
                    return Result.Failure<int>(
                        BidErrors.InvalidItems
                    );
                }


                orderItems.Add(
                    new OrderItem
                    {
                        RfqItemId =
                            rfqItem.Id,

                        ProductId =
                            rfqItem.ProductId,

                        ProductVariantId =
                            rfqItem.ProductVariantId,

                        UnitId =
                            rfqItem.UnitId,

                        Quantity =
                            rfqItem.Quantity,

                        UnitPrice =
                            bidItem.UnitPrice,

                        TotalPrice =
                            bidItem.TotalPrice
                    }
                );
            }


            // ==========================================
            // Order + Payment
            // ==========================================

            var order =
                new Order
                {
                    Number =
                        GenerateOrderNumber(),

                    RfqId =
                        rfq.Id,

                    BidId =
                        selectedBid.Id,

                    ProjectId =
                        rfq.ProjectId,

                    VendorId =
                        selectedBid.VendorId,

                    AcceptedByAccountUserId =
                        accountUser.Id,

                    Status =
                        OrderStatus.Confirmed,

                    Subtotal =
                        selectedBid.Subtotal,

                    ShippingCost =
                        selectedBid.ShippingCost,

                    TotalAmount =
                        selectedBid.TotalAmount,

                    CommissionRate =
                        commission.Rate,

                    CommissionAmount =
                        commission.CommissionAmount,

                    VendorNetAmount =
                        commission.VendorNetAmount,

                    CreatedAt =
                        now,

                    Items =
                        orderItems,

                    StatusHistory =
                    [
                        new OrderStatusHistory
                        {
                            OldStatus =
                                null,

                            NewStatus =
                                OrderStatus.Confirmed,

                            ChangedByUserId =
                                currentUserId,

                            ChangedAt =
                                now
                        }
                    ],

                    Payment =
                        new Payment
                        {
                            Amount =
                                selectedBid.TotalAmount,

                            Currency =
                                "EGP",

                            Status =
                                PaymentStatus.Pending,

                            PaidAmount =
                                0,

                            RefundedAmount =
                                0,

                            CreatedAt =
                                now
                        }
                };


            await _unitOfWork
                .Orders
                .InsertAsync(
                    order
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                order.Id
            );
        }


        // =====================================================
        // Helpers
        // =====================================================

        private async Task<Vendor?>
            GetApprovedVendorAsync(
                int currentUserId)
        {
            return await _unitOfWork
                .Vendors
                .FirstOrDefaultAsync(
                    x =>
                        x.OwnerUserId ==
                            currentUserId
                        &&
                        x.IsActive
                        &&
                        x.Status ==
                            VendorStatus.Approved
                );
        }


        private async Task<CustomerAccountUser?>
            GetCustomerAccessAsync(
                Rfq rfq,
                int currentUserId)
        {
            var accountUser =
                await _unitOfWork
                    .CustomerAccountUsers
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId ==
                                currentUserId
                            &&
                            x.CustomerAccountId ==
                                rfq.Project.CustomerAccountId
                            &&
                            x.IsActive
                    );


            if (accountUser is null)
            {
                return null;
            }


            if (
                accountUser.Role ==
                CustomerAccountUserRole.Owner
            )
            {
                return accountUser;
            }


            var hasProjectAccess =
                await _unitOfWork
                    .ProjectMembers
                    .AnyAsync(
                        x =>
                            x.ProjectId ==
                                rfq.ProjectId
                            &&
                            x.CustomerAccountUserId ==
                                accountUser.Id
                            &&
                            x.IsActive
                    );


            return hasProjectAccess
                ? accountUser
                : null;
        }


        private static Result<BidCalculatedData>
            CalculateBid(
                Rfq rfq,
                BidInput input)
        {
            var now =
                DateTime.UtcNow;


            if (input.ValidUntil <= now)
            {
                return Result
                    .Failure<BidCalculatedData>(
                        BidErrors.InvalidValidity
                    );
            }


            if (
                input.LeadTimeDays < 0
                ||
                input.ShippingCost < 0
            )
            {
                return Result
                    .Failure<BidCalculatedData>(
                        BidErrors.InvalidPrice
                    );
            }


            if (
                input.ShippingCostType !=
                    ShippingCostType.Included
                &&
                input.ShippingCostType !=
                    ShippingCostType.Separate
            )
            {
                return Result
                    .Failure<BidCalculatedData>(
                        BidErrors.InvalidShippingCostType
                    );
            }


            var submittedItems =
                input.Items ??
                new List<BidItemInput>();


            var bidItems =
                new List<BidItem>();


            decimal subtotal;


            // ==========================================
            // RFQ has line items
            // ==========================================

            if (rfq.Items.Count > 0)
            {
                if (
                    submittedItems.Count !=
                    rfq.Items.Count
                )
                {
                    return Result
                        .Failure<BidCalculatedData>(
                            BidErrors.InvalidItems
                        );
                }


                var distinctIds =
                    submittedItems
                        .Select(
                            x => x.RfqItemId
                        )
                        .Distinct()
                        .ToList();


                /*
                 * Prevent duplicate RfqItemId.
                 */
                if (
                    distinctIds.Count !=
                    submittedItems.Count
                )
                {
                    return Result
                        .Failure<BidCalculatedData>(
                            BidErrors.InvalidItems
                        );
                }


                var rfqItemIds =
                    rfq.Items
                        .Select(
                            x => x.Id
                        )
                        .OrderBy(
                            x => x
                        )
                        .ToList();


                var submittedItemIds =
                    distinctIds
                        .OrderBy(
                            x => x
                        )
                        .ToList();


                if (
                    !rfqItemIds.SequenceEqual(
                        submittedItemIds
                    )
                )
                {
                    return Result
                        .Failure<BidCalculatedData>(
                            BidErrors.InvalidItems
                        );
                }


                foreach (var requestItem in submittedItems)
                {
                    if (requestItem.UnitPrice <= 0)
                    {
                        return Result
                            .Failure<BidCalculatedData>(
                                BidErrors.InvalidPrice
                            );
                    }


                    var rfqItem =
                        rfq.Items.First(
                            x =>
                                x.Id ==
                                requestItem.RfqItemId
                        );


                    var unitPrice =
                        decimal.Round(
                            requestItem.UnitPrice,
                            2,
                            MidpointRounding
                                .AwayFromZero
                        );


                    var totalPrice =
                        decimal.Round(
                            rfqItem.Quantity *
                            unitPrice,
                            2,
                            MidpointRounding
                                .AwayFromZero
                        );


                    bidItems.Add(
                        new BidItem
                        {
                            RfqItemId =
                                rfqItem.Id,

                            UnitPrice =
                                unitPrice,

                            TotalPrice =
                                totalPrice,

                            Notes =
                                Normalize(
                                    requestItem.Notes
                                )
                        }
                    );
                }


                subtotal =
                    decimal.Round(
                        bidItems.Sum(
                            x => x.TotalPrice
                        ),
                        2,
                        MidpointRounding
                            .AwayFromZero
                    );
            }


            // ==========================================
            // BOQ-only RFQ
            // ==========================================

            else
            {
                /*
                 * A BOQ-only RFQ must not send
                 * random item ids.
                 */
                if (submittedItems.Count > 0)
                {
                    return Result
                        .Failure<BidCalculatedData>(
                            BidErrors.InvalidItems
                        );
                }


                if (
                    !input.OverallPrice.HasValue
                    ||
                    input.OverallPrice.Value <= 0
                )
                {
                    return Result
                        .Failure<BidCalculatedData>(
                            BidErrors.InvalidPrice
                        );
                }


                subtotal =
                    decimal.Round(
                        input.OverallPrice.Value,
                        2,
                        MidpointRounding
                            .AwayFromZero
                    );
            }


            // ==========================================
            // Shipping
            // ==========================================

            var shippingCost =
                input.ShippingCostType ==
                    ShippingCostType.Separate
                    ? decimal.Round(
                        input.ShippingCost,
                        2,
                        MidpointRounding
                            .AwayFromZero
                    )
                    : 0m;


            var totalAmount =
                decimal.Round(
                    subtotal +
                    shippingCost,
                    2,
                    MidpointRounding
                        .AwayFromZero
                );


            return Result.Success(
                new BidCalculatedData(
                    bidItems,
                    subtotal,
                    shippingCost,
                    totalAmount
                )
            );
        }


        private static BidInput MapInput(
            SubmitBidRequest request)
        {
            return new BidInput(
                request.ShippingCostType,
                request.ShippingCost,
                request.LeadTimeDays,
                request.ValidUntil,
                request.OverallPrice,
                request.Items?
                    .Select(
                        x =>
                            new BidItemInput(
                                x.RfqItemId,
                                x.UnitPrice,
                                x.Notes
                            )
                    )
                    .ToList()
                    ??
                    new List<BidItemInput>()
            );
        }


        private static BidInput MapInput(
            UpdateBidRequest request)
        {
            return new BidInput(
                request.ShippingCostType,
                request.ShippingCost,
                request.LeadTimeDays,
                request.ValidUntil,
                request.OverallPrice,
                request.Items?
                    .Select(
                        x =>
                            new BidItemInput(
                                x.RfqItemId,
                                x.UnitPrice,
                                x.Notes
                            )
                    )
                    .ToList()
                    ??
                    new List<BidItemInput>()
            );
        }


        private static VendorBidResponse MapVendorBid(
            Bid bid)
        {
            return new VendorBidResponse(
                bid.Id,
                bid.RfqId,
                bid.Status,
                bid.Subtotal,
                bid.ShippingCostType,
                bid.ShippingCost,
                bid.TotalAmount,
                bid.LeadTimeDays,
                bid.PaymentTerms,
                bid.ValidUntil,
                bid.Notes,
                bid.SubmittedAt,
                bid.UpdatedAt,
                bid.WithdrawnAt,
                bid.Items
                    .Select(
                        x =>
                            new VendorBidItemResponse(
                                x.RfqItemId,
                                x.UnitPrice,
                                x.TotalPrice,
                                x.Notes
                            )
                    )
                    .ToList()
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


        private static string GenerateOrderNumber()
        {
            return
                $"ORD-{DateTime.UtcNow:yyyyMMdd}-" +
                $"{Guid.NewGuid():N}"[..8]
                    .ToUpperInvariant();
        }


        private sealed record BidInput(
            ShippingCostType ShippingCostType,
            decimal ShippingCost,
            int LeadTimeDays,
            DateTime ValidUntil,
            decimal? OverallPrice,
            List<BidItemInput> Items
        );


        private sealed record BidItemInput(
            int RfqItemId,
            decimal UnitPrice,
            string? Notes
        );


        private sealed record BidCalculatedData(
            List<BidItem> Items,
            decimal Subtotal,
            decimal ShippingCost,
            decimal TotalAmount
        );
    }
}