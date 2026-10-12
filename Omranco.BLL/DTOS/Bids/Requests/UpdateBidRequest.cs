using Omranco.Domain.Enums.Bids;

namespace Omranco.BLL.DTOS.Bids.Requests;

public sealed record UpdateBidRequest(
    ShippingCostType ShippingCostType,
    decimal ShippingCost,
    int LeadTimeDays,
    string? PaymentTerms,
    DateTime ValidUntil,
    string? Notes,
    decimal? OverallPrice,
    List<SubmitBidItemRequest> Items
);