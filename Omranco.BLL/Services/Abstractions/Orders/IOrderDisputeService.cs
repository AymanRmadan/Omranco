using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Orders.Disputes;

namespace Omranco.BLL.Services.Abstractions.OrderServices;

public interface IOrderDisputeService
{
    Task<Result<OpenOrderDisputeResponse>> OpenAsync(int orderId, int currentUserId, OpenOrderDisputeRequest request);
}