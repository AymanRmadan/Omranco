using Omranco.Domain.Enums.Bids;

namespace Omranco.BLL.DTOS.Bids.Responses;

public sealed record VendorBidResponse(
    int Id,
    int RfqId,
    BidStatus Status,
    decimal Subtotal,
    ShippingCostType ShippingCostType,
    decimal ShippingCost,
    decimal TotalAmount,
    int LeadTimeDays,
    string? PaymentTerms,
    DateTime ValidUntil,
    string? Notes,
    DateTime SubmittedAt,
    DateTime? UpdatedAt,
    DateTime? WithdrawnAt,
    List<VendorBidItemResponse> Items
);