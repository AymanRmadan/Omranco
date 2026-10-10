using Omranco.Domain.Enums.Vendors;

namespace Omranco.Domain.Entities.Vendors
{
    public sealed class VendorPayoutAccount : BaseEntity<int>
    {

        public int VendorId { get; set; }

        public PayoutMethod Method { get; set; }

        public string? AccountHolderName { get; set; }

        // Bank
        public string? BankName { get; set; }

        public string? AccountNumber { get; set; }

        public string? Iban { get; set; }

        // Wallet
        public string? WalletProvider { get; set; }

        public string? WalletPhoneNumber { get; set; }

        public bool IsDefault { get; set; }

        public bool IsVerified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Vendor Vendor { get; set; } = null!;
    }
}