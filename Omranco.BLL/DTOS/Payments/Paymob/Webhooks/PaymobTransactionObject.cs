using System.Text.Json.Serialization;

namespace Omranco.BLL.DTOS.Payments.Paymob.Webhooks;

public sealed record PaymobTransactionObject(
    [property: JsonPropertyName("id")]
    long Id,

    [property: JsonPropertyName("pending")]
    bool Pending,

    [property: JsonPropertyName("amount_cents")]
    long AmountCents,

    [property: JsonPropertyName("success")]
    bool Success,

    [property: JsonPropertyName("is_auth")]
    bool IsAuth,

    [property: JsonPropertyName("is_capture")]
    bool IsCapture,

    [property: JsonPropertyName("is_standalone_payment")]
    bool IsStandalonePayment,

    [property: JsonPropertyName("is_voided")]
    bool IsVoided,

    [property: JsonPropertyName("is_refunded")]
    bool IsRefunded,

    [property: JsonPropertyName("is_3d_secure")]
    bool Is3DSecure,

    [property: JsonPropertyName("integration_id")]
    long IntegrationId,

    [property: JsonPropertyName("has_parent_transaction")]
    bool HasParentTransaction,

    [property: JsonPropertyName("order")]
    PaymobCallbackOrder Order,

    [property: JsonPropertyName("created_at")]
    string CreatedAt,

    [property: JsonPropertyName("currency")]
    string Currency,

    [property: JsonPropertyName("error_occured")]
    bool ErrorOccurred,

    [property: JsonPropertyName("owner")]
    long Owner,

    [property: JsonPropertyName("source_data")]
    PaymobSourceData SourceData,

    [property: JsonPropertyName("is_refund")]
    bool IsRefund,

    [property: JsonPropertyName("refunded_amount_cents")]
    long? RefundedAmountCents,

    [property: JsonPropertyName("data")]
    PaymobTransactionData? Data,
    [property: JsonPropertyName("parent_transaction")]
string? ParentTransaction
);