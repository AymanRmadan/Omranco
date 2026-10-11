using Omranco.Domain.Enums.Vendors;

namespace Omranco.BLL.DTOS.Vendors.PayoutAccounts;

public sealed record CreateVendorPayoutAccountRequest(
    PayoutMethod Method,
    string? AccountHolderName,
    string? BankName,
    string? AccountNumber,
    string? Iban,
    string? WalletProvider,
    string? WalletPhoneNumber,
    bool IsDefault
);