using Omranco.BLL.DTOS.Payments.Paymob.Webhooks;

namespace Omranco.BLL.Services.Abstractions.PaymentServices;

public interface IPaymobHmacService
{
    bool IsValidTransactionHmac(PaymobTransactionObject transaction, string receivedHmac);
}