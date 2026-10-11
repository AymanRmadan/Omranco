using System.Text.Json.Serialization;

namespace Omranco.BLL.DTOS.Payments.Paymob.Webhooks;

public sealed record PaymobCallbackOrder(
    [property: JsonPropertyName("id")]
    long Id
);