namespace Omranco.BLL.DTOS.Rfqs;

public sealed record CreateRfqRequest(
    DateOnly? DesiredDeliveryDate,
    DateTime DeadlineAt,
    string? Notes,
    List<int> CategoryIds,
    List<CreateRfqItemRequest> Items
);