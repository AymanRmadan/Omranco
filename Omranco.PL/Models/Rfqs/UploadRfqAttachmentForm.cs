using Omranco.Domain.Enums.Rfqs;

namespace Omranco.PL.Models.Rfqs;

public sealed record UploadRfqAttachmentForm(
    RfqAttachmentType Type,
    IFormFile File
);