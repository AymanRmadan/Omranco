using System.Text.Json.Serialization;

namespace Omranco.BLL.DTOS.Payments.Paymob.Webhooks;

public sealed record PaymobSourceData(
    [property: JsonPropertyName("pan")]
    string? Pan,

    [property: JsonPropertyName("sub_type")]
    string? SubType,

    [property: JsonPropertyName("type")]
    string? Type
);