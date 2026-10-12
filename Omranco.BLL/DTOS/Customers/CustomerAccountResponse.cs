using Omranco.Domain.Enums;

namespace Omranco.BLL.DTOS.Customers;

public sealed record CustomerAccountResponse(
    int Id,
    string Name,
    CustomerAccountType Type,
    bool IsActive,
    DateTime CreatedAt
);