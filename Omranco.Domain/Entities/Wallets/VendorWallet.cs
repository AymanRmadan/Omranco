using Omranco.Domain.Entities.Vendors;

namespace Omranco.Domain.Entities.Wallets
{
    public sealed class VendorWallet : BaseEntity<int>
    {
        public int VendorId { get; set; }

        public decimal Balance { get; set; }

        public decimal TotalEarned { get; set; }

        public decimal TotalSettled { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();


        // Navigation Properties

        public Vendor Vendor { get; set; } = null!;

        public ICollection<VendorWalletTransaction> Transactions { get; set; } = new List<VendorWalletTransaction>();
    }
}