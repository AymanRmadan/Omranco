namespace Omranco.BLL;

public sealed class PaymobOptions
{
    public const string SectionName = "Paymob";

    [Required]
    [Url]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    [Url]
    public string CheckoutBaseUrl { get; init; } = string.Empty;

    [Required]
    public string SecretKey { get; init; } = string.Empty;

    [Required]
    public string PublicKey { get; init; } = string.Empty;

    [Required]
    public string HmacSecret { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CardIntegrationId { get; init; }

    public int? WalletIntegrationId { get; init; }

    [Required]
    [Url]
    public string NotificationUrl { get; init; } = string.Empty;

    [Required]
    [Url]
    public string RedirectionUrl { get; init; } = string.Empty;

    [Range(60, 86400)]
    public int IntentionExpirationSeconds { get; init; } = 3600;
}