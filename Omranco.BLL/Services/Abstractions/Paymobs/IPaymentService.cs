using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Payments.Paymob.Webhooks;
using Omranco.BLL.DTOS.Payments.Refunds;
using Omranco.BLL.DTOS.Payments.Requests;
using Omranco.BLL.DTOS.Payments.Responses;

namespace Omranco.BLL.Services.Abstractions.PaymentServices;

public interface IPaymentService
{
    Task<Result<StartPaymentResponse>> StartPaymentAsync(int orderId, int currentUserId, StartPaymentRequest request);
    Task<Result> HandlePaymobWebhookAsync(PaymobTransactionCallback callback, string hmac);

    Task<Result<RequestRefundResponse>> RequestRefundAsync(int orderId, int currentUserId, RequestRefundRequest request);
}