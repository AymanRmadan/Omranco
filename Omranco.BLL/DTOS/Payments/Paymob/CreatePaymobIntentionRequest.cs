using Omranco.Domain.Enums.Payments;

namespace Omranco.BLL.DTOS.Payments.Paymob;

public sealed record CreatePaymobIntentionRequest(
    decimal Amount,
    string Currency,
    PaymentMethod PaymentMethod,
    string Reference,
    string ItemName,
    string? ItemDescription,
    PaymobBillingData BillingData
);