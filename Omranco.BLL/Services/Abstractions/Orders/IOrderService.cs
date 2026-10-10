using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Orders.Responses;
using Omranco.Domain.Enums.Orders;

namespace Omranco.BLL.Services.Abstractions.OrderServices
{
    public interface IOrderService
    {
        Task<Result<VendorOrderDetailsResponse>> GetVendorOrderDetailsAsync(int orderId, int currentUserId);

        Task<Result> UpdateStatusAsync(int orderId, int currentUserId, OrderStatus newStatus);
    }
}