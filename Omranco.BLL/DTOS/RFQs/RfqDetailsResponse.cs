using Omranco.Domain.Enums.Rfqs;

namespace Omranco.BLL.DTOS.Rfqs;

public sealed record RfqDetailsResponse(
    int Id,
    string Number,
    int ProjectId,
    RfqStatus Status,
    DateOnly? DesiredDeliveryDate,
    DateTime? DeadlineAt,
    string? Notes,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? ClosedAt,
    List<int> CategoryIds,
    List<RfqItemResponse> Items,
    List<RfqAttachmentResponse> Attachments
);