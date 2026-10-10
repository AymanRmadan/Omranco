using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Bids.Requests;

namespace Omranco.BLL.Services.Abstractions.BidServices
{
    public interface IBidService
    {
        Task<Result<int>> SubmitAsync(
            int rfqId,
            int currentUserId,
            SubmitBidRequest request);
    }
}