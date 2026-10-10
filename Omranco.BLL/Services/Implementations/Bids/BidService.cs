using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Bids.Requests;
using Omranco.BLL.Services.Abstractions.BidServices;
using Omranco.Domain.Entities.Bids;
using Omranco.Domain.Enums.Bids;
using Omranco.Domain.Enums.Rfqs;
using Omranco.Domain.Enums.Vendors;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.BidServices
{
    public sealed class BidService
        : IBidService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BidService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Result<int>> SubmitAsync(
            int rfqId,
            int currentUserId,
            SubmitBidRequest request)
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
    }
}