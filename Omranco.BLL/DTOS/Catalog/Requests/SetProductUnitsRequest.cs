namespace Omranco.BLL.DTOS.Catalog.Requests;

public sealed record SetProductUnitsRequest(
    List<ProductUnitRequest> Units
);