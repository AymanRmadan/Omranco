using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Orders.Cancellations;

namespace Omranco.BLL.Services.Abstractions.OrderServices;

public interface IOrderCancellationService
{
    Task<Result<CancelOrderResponse>> CancelAsync(int orderId, int currentUserId, CancelOrderRequest request);
}