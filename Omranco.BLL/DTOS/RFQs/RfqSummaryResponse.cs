using Omranco.Domain.Enums.Rfqs;

namespace Omranco.BLL.DTOS.Rfqs;

public sealed record RfqSummaryResponse(
    int Id,
    string Number,
    int ProjectId,
    RfqStatus Status,
    DateOnly? DesiredDeliveryDate,
    DateTime? DeadlineAt,
    int ItemsCount,
    int AttachmentsCount,
    DateTime CreatedAt
);