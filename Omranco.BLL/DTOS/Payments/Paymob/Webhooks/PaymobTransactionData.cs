using System.Text.Json.Serialization;

namespace Omranco.BLL.DTOS.Payments.Paymob.Webhooks;

public sealed record PaymobTransactionData(
    [property: JsonPropertyName("message")]
    string? Message
);