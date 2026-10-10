using Omranco.BLL.Commons.ResponseResults;

namespace Omranco.BLL.Services.Abstractions.WalletServices
{
    public interface IVendorWalletService
    {
        Task<Result<int>> CreditOrderEarningAsync(int orderId);
    }
}