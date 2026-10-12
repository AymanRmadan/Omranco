using Omranco.Domain.Enums.Rfqs;

namespace Omranco.BLL.DTOS.Rfqs;

public sealed record RfqAttachmentResponse(
    int Id,
    RfqAttachmentType Type,
    string FilePath,
    string OriginalFileName,
    string? ContentType,
    long? FileSizeBytes,
    DateTime UploadedAt
);