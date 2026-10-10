namespace Omranco.BLL.DTOS.Bids.Responses
{
    public record BidComparisonResponse(
        int BidId,
        int OfferNumber,
        decimal VendorRating,
        int VendorRatingsCount,
        decimal Subtotal,
        string ShippingCostType,
        decimal ShippingCost,
        decimal TotalAmount,
        int LeadTimeDays,
        string? PaymentTerms,
        DateTime ValidUntil,
        bool IsExpired,
        string? Notes,
        List<BidComparisonItemResponse> Items);

}