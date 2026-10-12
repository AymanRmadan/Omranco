using Omranco.Domain.Enums;

namespace Omranco.BLL.DTOS.Customers;

public sealed record CustomerAccountMemberResponse(
    int AccountUserId,
    int UserId,
    string Name,
    string? PhoneNumber,
    CustomerAccountUserRole Role,
    bool IsActive
);