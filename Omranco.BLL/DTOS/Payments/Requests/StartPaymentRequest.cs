using Omranco.BLL.DTOS.Payments.Paymob;
using Omranco.Domain.Enums.Payments;

namespace Omranco.BLL.DTOS.Payments.Requests;

public sealed record StartPaymentRequest(
    PaymentMethod PaymentMethod,
    PaymobBillingData BillingData
);