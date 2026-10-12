using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Bids.Requests;
using Omranco.BLL.DTOS.Bids.Responses;

namespace Omranco.BLL.Services.Abstractions.BidServices
{
    public interface IBidService
    {
        // Vendor Invitations

        Task<Result<List<VendorRfqInvitationResponse>>>
            GetMyInvitationsAsync(
                int currentUserId);

        Task<Result<VendorRfqInvitationDetailsResponse>>
            GetInvitationAsync(
                int rfqId,
                int currentUserId);

        Task<Result>
            DeclineInvitationAsync(
                int rfqId,
                int currentUserId);


        // Vendor Bid

        Task<Result<int>>
            SubmitAsync(
                int rfqId,
                int currentUserId,
                SubmitBidRequest request);

        Task<Result<VendorBidResponse>>
            GetMyBidAsync(
                int rfqId,
                int currentUserId);

        Task<Result>
            UpdateAsync(
                int rfqId,
                int currentUserId,
                UpdateBidRequest request);

        Task<Result>
            WithdrawAsync(
                int rfqId,
                int currentUserId);


        // Customer

        Task<Result<List<BidComparisonResponse>>>
            GetComparisonAsync(
                int rfqId,
                int currentUserId);

        Task<Result<int>>
            AcceptAsync(
                int rfqId,
                int bidId,
                int currentUserId);
    }
}