using Omranco.Domain.Enums.Rfqs;

namespace Omranco.BLL.DTOS.Bids.Responses;

public sealed record VendorRfqInvitationDetailsResponse(
    int InvitationId,
    int RfqId,
    string RfqNumber,
    RfqInvitationStatus InvitationStatus,
    DateTime? DeadlineAt,
    DateOnly? DesiredDeliveryDate,
    string? Notes,
    DateTime SentAt,
    DateTime? ViewedAt,
    List<VendorRfqItemResponse> Items
);