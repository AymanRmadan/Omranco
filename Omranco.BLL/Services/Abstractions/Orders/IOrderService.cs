using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Orders.Responses;
using Omranco.Domain.Enums.Orders;

namespace Omranco.BLL.Services.Abstractions.OrderServices
{
    public interface IOrderService
    {
        // Vendor

        Task<Result<List<VendorOrderSummaryResponse>>>
            GetVendorOrdersAsync(
                int currentUserId,
                OrderStatus? status);

        Task<Result<VendorOrderDetailsResponse>>
            GetVendorOrderDetailsAsync(
                int orderId,
                int currentUserId);

        Task<Result>
            UpdateStatusAsync(
                int orderId,
                int currentUserId,
                OrderStatus newStatus);


        // Customer

        Task<Result<List<CustomerOrderSummaryResponse>>>
            GetCustomerOrdersAsync(
                int customerAccountId,
                int currentUserId,
                OrderStatus? status);

        Task<Result<CustomerOrderDetailsResponse>>
            GetCustomerOrderDetailsAsync(
                int orderId,
                int currentUserId);
    }
}