namespace Omranco.BLL.DTOS.Catalog.Responses;

public sealed record ProductVariantValueResponse(
    int ProductAttributeId,
    string AttributeName,
    int ProductAttributeOptionId,
    string OptionValue
);