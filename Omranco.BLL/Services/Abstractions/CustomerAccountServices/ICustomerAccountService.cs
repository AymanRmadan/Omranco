using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Customers;

namespace Omranco.BLL.Services.Abstractions.CustomerServices;

public interface ICustomerAccountService
{
    Task<Result<CustomerAccountResponse>> CreateAsync(
        int currentUserId,
        CreateCustomerAccountRequest request);

    Task<Result<List<CustomerAccountResponse>>> GetMyAccountsAsync(
        int currentUserId);

    Task<Result<List<CustomerAccountMemberResponse>>> GetMembersAsync(
        int accountId,
        int currentUserId);

    Task<Result<CustomerAccountMemberResponse>> AddMemberAsync(
        int accountId,
        int currentUserId,
        AddCustomerAccountMemberRequest request);

    Task<Result> RemoveMemberAsync(
        int accountId,
        int accountUserId,
        int currentUserId);
}