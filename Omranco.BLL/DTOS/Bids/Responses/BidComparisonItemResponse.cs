namespace Omranco.BLL.DTOS.Bids.Responses
{
    public record BidComparisonItemResponse(
        int? RfqItemId,
        decimal UnitPrice,
        decimal TotalPrice,
        string? Notes);

}