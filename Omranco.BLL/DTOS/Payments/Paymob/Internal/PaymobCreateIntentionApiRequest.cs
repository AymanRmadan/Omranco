using System.Text.Json.Serialization;

namespace Omranco.BLL.DTOS.Payments.Paymob.Internal;

public sealed record PaymobCreateIntentionApiRequest(
    [property: JsonPropertyName("amount")]
    long Amount,

    [property: JsonPropertyName("currency")]
    string Currency,

    [property: JsonPropertyName("payment_methods")]
    int[] PaymentMethods,

    [property: JsonPropertyName("items")]
    PaymobItemApiRequest[] Items,

    [property: JsonPropertyName("billing_data")]
    PaymobBillingDataApiRequest BillingData,

    [property: JsonPropertyName("special_reference")]
    string SpecialReference,

    [property: JsonPropertyName("expiration")]
    int Expiration,

    [property: JsonPropertyName("notification_url")]
    string? NotificationUrl,

    [property: JsonPropertyName("redirection_url")]
    string RedirectionUrl
);