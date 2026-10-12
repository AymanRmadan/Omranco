namespace Omranco.BLL.DTOS.Rfqs;

public sealed record UpdateRfqRequest(
    DateOnly? DesiredDeliveryDate,
    DateTime DeadlineAt,
    string? Notes,
    List<int> CategoryIds,
    List<CreateRfqItemRequest> Items
);