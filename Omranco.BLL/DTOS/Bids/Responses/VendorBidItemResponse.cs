namespace Omranco.BLL.DTOS.Bids.Responses;

public sealed record VendorBidItemResponse(
    int RfqItemId,
    decimal UnitPrice,
    decimal TotalPrice,
    string? Notes
);