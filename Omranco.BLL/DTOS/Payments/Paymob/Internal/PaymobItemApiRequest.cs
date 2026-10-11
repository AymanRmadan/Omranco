using System.Text.Json.Serialization;

namespace Omranco.BLL.DTOS.Payments.Paymob.Internal;

public sealed record PaymobItemApiRequest(
    [property: JsonPropertyName("name")]
    string Name,

    [property: JsonPropertyName("amount")]
    long Amount,

    [property: JsonPropertyName("description")]
    string Description,

    [property: JsonPropertyName("quantity")]
    int Quantity
);