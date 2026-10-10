using Omranco.Domain.Enums.Rfqs;

namespace Omranco.Domain.Entities.Rfqs
{
    public sealed class RfqAttachment : BaseEntity<int>
    {

        public int RfqId { get; set; }

        public RfqAttachmentType Type { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string? ContentType { get; set; }

        public long? FileSizeBytes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties

        public Rfq Rfq { get; set; } = null!;
    }
}