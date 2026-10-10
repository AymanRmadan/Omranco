namespace Omranco.BLL.DTOS.Bids.Requests
{
    public sealed record SubmitBidItemRequest(
        int RfqItemId,
        decimal UnitPrice,
        string? Notes)
    ;
}