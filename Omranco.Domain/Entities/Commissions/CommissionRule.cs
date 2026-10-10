namespace Omranco.Domain.Entities.Commissions
{
    public sealed class CommissionRule : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty;

        // مثال: 5 معناها 5%
        public decimal Percentage { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}