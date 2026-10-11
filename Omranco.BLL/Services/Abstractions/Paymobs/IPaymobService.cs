using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Payments.Paymob;

namespace Omranco.BLL.Services.Abstractions.PaymentServices;

public interface IPaymobService
{
    Task<Result<CreatePaymobIntentionResponse>> CreateIntentionAsync(CreatePaymobIntentionRequest request);

    Task<Result> RefundAsync(long transactionId, decimal amount);
}