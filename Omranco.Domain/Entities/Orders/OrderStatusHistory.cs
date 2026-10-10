using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Enums.Orders;

namespace Omranco.Domain.Entities.Orders
{
    public sealed class OrderStatusHistory : BaseEntity<int>
    {
        public int OrderId { get; set; }

        public OrderStatus? OldStatus { get; set; }

        public OrderStatus NewStatus { get; set; }

        public int ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; }
            = DateTime.UtcNow;


        // Navigation Properties

        public Order Order { get; set; } = null!;

        public ApplicationUser ChangedByUser { get; set; } = null!;
    }
}