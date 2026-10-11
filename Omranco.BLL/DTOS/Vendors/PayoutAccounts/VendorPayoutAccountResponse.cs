using Omranco.Domain.Enums.Vendors;

namespace Omranco.BLL.DTOS.Vendors.PayoutAccounts;

public sealed record VendorPayoutAccountResponse(
    int Id,
    PayoutMethod Method,
    string? AccountHolderName,
    string? BankName,
    string? AccountNumber,
    string? Iban,
    string? WalletProvider,
    string? WalletPhoneNumber,
    bool IsDefault,
    bool IsVerified,
    DateTime CreatedAt
);