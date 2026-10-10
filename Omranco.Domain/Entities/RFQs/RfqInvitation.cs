using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Enums.Rfqs;

namespace Omranco.Domain.Entities.Rfqs
{
    public sealed class RfqInvitation : BaseEntity<int>
    {

        public int RfqId { get; set; }

        public int VendorId { get; set; }

        public RfqInvitationStatus Status { get; set; }
            = RfqInvitationStatus.Sent;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? ViewedAt { get; set; }

        public DateTime? RespondedAt { get; set; }

        public DateTime? DeclinedAt { get; set; }


        // Navigation Properties

        public Rfq Rfq { get; set; } = null!;

        public Vendor Vendor { get; set; } = null!;
    }
}