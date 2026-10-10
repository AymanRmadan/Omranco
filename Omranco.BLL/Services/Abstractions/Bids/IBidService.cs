using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Bids.Requests;
using Omranco.BLL.DTOS.Bids.Responses;

namespace Omranco.BLL.Services.Abstractions.BidServices
{
    public interface IBidService
    {
        Task<Result<int>> SubmitAsync(int rfqId, int currentUserId, SubmitBidRequest request);

        Task<Result<List<BidComparisonResponse>>> GetComparisonAsync(int rfqId, int currentUserId);

        Task<Result<int>> AcceptAsync(int rfqId, int bidId, int currentUserId);
    }
}