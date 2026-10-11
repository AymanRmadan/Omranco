using Omranco.Domain.Enums.Vendors;

namespace Omranco.BLL.DTOS.Vendors.Payouts;

public sealed record VendorPayoutResponse(
    int Id,
    string Reference,
    decimal Amount,
    VendorPayoutRequestStatus Status,
    PayoutMethod PayoutMethod,
    string? AccountHolderName,
    string? BankName,
    string? IBAN,
    string? WalletProvider,
    string? WalletPhoneNumber,
    string? ExternalReference,
    string? ReviewNotes,
    string? FailureReason,
    DateTime RequestedAt,
    DateTime? ReviewedAt,
    DateTime? ProcessingAt,
    DateTime? CompletedAt
);