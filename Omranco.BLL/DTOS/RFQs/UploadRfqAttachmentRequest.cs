using Omranco.Domain.Enums.Rfqs;

namespace Omranco.BLL.DTOS.Rfqs;

public sealed record UploadRfqAttachmentRequest(
    RfqAttachmentType Type,
    string FileName,
    string ContentType,
    long Length,
    Stream Content
);