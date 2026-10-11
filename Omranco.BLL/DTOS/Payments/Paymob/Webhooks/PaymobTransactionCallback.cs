using System.Text.Json.Serialization;

namespace Omranco.BLL.DTOS.Payments.Paymob.Webhooks;

public sealed record PaymobTransactionCallback(
    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("obj")]
    PaymobTransactionObject Obj
);