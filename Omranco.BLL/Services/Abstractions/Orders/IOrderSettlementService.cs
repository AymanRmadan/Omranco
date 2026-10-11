using Omranco.BLL.Commons.ResponseResults;

namespace Omranco.BLL.Services.Abstractions.OrderServices;

public interface IOrderSettlementService
{
    Task<Result> TrySettleAsync(int orderId);
}