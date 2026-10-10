using Omranco.BLL.Commons.ResponseResults;

namespace Omranco.BLL.Services.Abstractions.RfqServices
{
    public interface IRfqMatchingService
    {
        Task<Result<List<int>>> FindMatchingVendorIdsAsync(int rfqId);
    }
}