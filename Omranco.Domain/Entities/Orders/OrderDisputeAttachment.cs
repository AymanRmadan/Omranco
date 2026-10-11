namespace Omranco.Domain.Entities.Orders;

public sealed class OrderDisputeAttachment : BaseEntity<int>
{
    public int OrderDisputeId { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long FileSizeBytes { get; set; }

    public DateTime UploadedAt { get; set; }
        = DateTime.UtcNow;


    public OrderDispute OrderDispute { get; set; } = null!;
}