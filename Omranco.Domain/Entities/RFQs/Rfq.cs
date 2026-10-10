using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Enums.Rfqs;

namespace Omranco.Domain.Entities.Rfqs
{
    public sealed class Rfq : BaseEntity<int>
    {

        public string Number { get; set; } = string.Empty;

        public int ProjectId { get; set; }

        public int CreatedByAccountUserId { get; set; }

        public RfqStatus Status { get; set; } = RfqStatus.Draft;

        public DateOnly? DesiredDeliveryDate { get; set; }

        public DateTime? DeadlineAt { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties

        public Project Project { get; set; } = null!;

        public CustomerAccountUser CreatedByAccountUser { get; set; } = null!;

        public ICollection<RfqItem> Items { get; set; }
            = new List<RfqItem>();

        public ICollection<RfqAttachment> Attachments { get; set; }
            = new List<RfqAttachment>();

        public ICollection<RfqInvitation> Invitations { get; set; }
    = new List<RfqInvitation>();

        public ICollection<RfqCategory> Categories { get; set; }
    = new List<RfqCategory>();
    }
}