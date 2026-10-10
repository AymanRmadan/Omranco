using Omranco.Domain.Entities.Bids;
using Omranco.Domain.Entities.Payments;
using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Entities.Rfqs;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Enums.Orders;

namespace Omranco.Domain.Entities.Orders
{
    public sealed class Order : BaseEntity<int>
    {
        public string Number { get; set; } = string.Empty;

        public int RfqId { get; set; }

        public int BidId { get; set; }

        public int ProjectId { get; set; }

        public int VendorId { get; set; }

        public int AcceptedByAccountUserId { get; set; }

        public OrderStatus Status { get; set; }
            = OrderStatus.Confirmed;

        public decimal Subtotal { get; set; }

        public decimal ShippingCost { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        public DateTime? InProgressAt { get; set; }

        public DateTime? ShippedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public DateTime? CancelledAt { get; set; }



        public decimal CommissionRate { get; set; }

        public decimal CommissionAmount { get; set; }

        public decimal VendorNetAmount { get; set; }


        public ICollection<OrderStatusHistory> StatusHistory { get; set; }
            = new List<OrderStatusHistory>();


        // Navigation Properties

        public Rfq Rfq { get; set; } = null!;

        public Bid Bid { get; set; } = null!;

        public Project Project { get; set; } = null!;

        public Vendor Vendor { get; set; } = null!;

        public CustomerAccountUser AcceptedByAccountUser { get; set; } = null!;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }

    }
}