using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Enums.Wallets;

namespace Omranco.Domain.Entities.Wallets
{
    public sealed class VendorWalletTransaction
        : BaseEntity<int>
    {
        public int VendorWalletId { get; set; }

        public int? OrderId { get; set; }

        public VendorWalletTransactionType Type { get; set; }

        public WalletTransactionDirection Direction { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }

        public string Reference { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;


        // Navigation Properties

        public VendorWallet VendorWallet { get; set; } = null!;

        public Order? Order { get; set; }
    }
}