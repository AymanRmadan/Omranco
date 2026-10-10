using Omranco.BLL.Commons.ResponseResults;

namespace Omranco.BLL.Services.Abstractions.RfqServices
{
    public interface IRfqService
    {
        Task<Result> SubmitAsync(int rfqId, int currentUserId);
    }
}