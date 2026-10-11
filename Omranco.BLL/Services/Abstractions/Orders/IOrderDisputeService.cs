using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Orders.Disputes;

namespace Omranco.BLL.Services.Abstractions.OrderServices;

public interface IOrderDisputeService
{
    Task<Result<OpenOrderDisputeResponse>> OpenAsync(
        int orderId,
        int currentUserId,
        OpenOrderDisputeRequest request);

    Task<Result<OrderDisputeReviewResponse>> StartReviewAsync(
        int disputeId,
        int currentUserId);

    Task<Result<OrderDisputeReviewResponse>> RejectAsync(
        int disputeId,
        int currentUserId,
        RejectOrderDisputeRequest request);

    Task<Result<ApproveDisputeRefundResponse>> ApproveRefundAsync(
    int disputeId,
    int currentUserId,
    ApproveDisputeRefundRequest request);
}