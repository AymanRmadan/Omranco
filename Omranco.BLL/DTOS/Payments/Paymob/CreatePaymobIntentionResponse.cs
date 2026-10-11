namespace Omranco.BLL.DTOS.Payments.Paymob;

public sealed record CreatePaymobIntentionResponse(
    string IntentionId,
    string PaymobOrderId,
    string ClientSecret,
    string CheckoutUrl
);