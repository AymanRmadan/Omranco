namespace Omranco.BLL.DTOS.Orders.Cancellations;

public sealed record CancelOrderRequest(
    string? Reason
);
