using System.Text.Json.Serialization;

namespace Omranco.BLL.DTOS.Payments.Paymob.Internal;

public sealed record PaymobCreateIntentionApiResponse(
    [property: JsonPropertyName("id")]
    string Id,

    [property: JsonPropertyName("intention_order_id")]
    long IntentionOrderId,

    [property: JsonPropertyName("client_secret")]
    string ClientSecret
);