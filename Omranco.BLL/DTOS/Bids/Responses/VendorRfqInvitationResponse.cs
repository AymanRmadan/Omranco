using Omranco.Domain.Enums.Rfqs;

namespace Omranco.BLL.DTOS.Bids.Responses;

public sealed record VendorRfqInvitationResponse(
    int InvitationId,
    int RfqId,
    string RfqNumber,
    RfqInvitationStatus Status,
    DateTime? DeadlineAt,
    DateTime SentAt,
    DateTime? ViewedAt,
    DateTime? RespondedAt,
    DateTime? DeclinedAt
);