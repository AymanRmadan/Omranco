using Omranco.Domain.Enums;

namespace Omranco.BLL.DTOS.Customers;

public sealed record CreateCustomerAccountRequest(
    string Name,
    CustomerAccountType Type
);