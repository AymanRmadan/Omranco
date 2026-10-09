using Omranco.Domain.Entities.Vendors;

namespace Omranco.Domain.Entities.Auths
{
    public sealed class ApplicationUser : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }


        // Navigation Properties
        public Vendor? OwnedVendor { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
            = new List<RefreshToken>();

        public ICollection<CustomerAccountUser> CustomerAccounts { get; set; } = new List<CustomerAccountUser>();
    }
}