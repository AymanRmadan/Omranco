using System.Text.Json.Serialization;

namespace Omranco.BLL.DTOS.Payments.Paymob.Internal;

public sealed record PaymobRefundApiRequest(
    [property: JsonPropertyName("transaction_id")]
    long TransactionId,

    [property: JsonPropertyName("amount_cents")]
    long AmountCents
);