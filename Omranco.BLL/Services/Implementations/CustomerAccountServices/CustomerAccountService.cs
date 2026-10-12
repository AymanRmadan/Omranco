using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Customers;
using Omranco.BLL.Services.Abstractions.CustomerServices;
using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Enums;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.CustomerServices;

public sealed class CustomerAccountService
    : ICustomerAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerAccountService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }


    public async Task<Result<CustomerAccountResponse>> CreateAsync(
        int currentUserId,
        CreateCustomerAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<CustomerAccountResponse>(
                CustomerAccountErrors.InvalidName
            );
        }


        var now = DateTime.UtcNow;


        var account =
            new CustomerAccount
            {
                Name =
                    request.Name.Trim(),

                Type =
                    request.Type,

                IsActive =
                    true,

                CreatedAt =
                    now,

                Users =
                [
                    new CustomerAccountUser
                    {
                        UserId =
                            currentUserId,

                        Role =
                            CustomerAccountUserRole.Owner,

                        IsActive =
                            true,

                        CreatedAt =
                            now
                    }
                ]
            };


        await _unitOfWork
            .CustomerAccounts
            .InsertAsync(account);


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success(
            Map(account)
        );
    }


    public async Task<Result<List<CustomerAccountResponse>>>
        GetMyAccountsAsync(
            int currentUserId)
    {
        var memberships =
            await _unitOfWork
                .CustomerAccountUsers
                .GetAllAsync(
                    filter:
                        x =>
                            x.UserId == currentUserId
                            &&
                            x.IsActive,

                    includes:
                    [
                        x => x.CustomerAccount
                    ]
                );


        var accounts =
            memberships
                .Where(
                    x =>
                        x.CustomerAccount.IsActive
                )
                .Select(
                    x => Map(x.CustomerAccount)
                )
                .ToList();


        return Result.Success(accounts);
    }


    public async Task<Result<List<CustomerAccountMemberResponse>>>
        GetMembersAsync(
            int accountId,
            int currentUserId)
    {
        var currentAccountUser =
            await GetActiveAccountUserAsync(
                accountId,
                currentUserId
            );


        if (currentAccountUser is null)
        {
            return Result
                .Failure<List<CustomerAccountMemberResponse>>(
                    CustomerAccountErrors.AccessDenied
                );
        }


        var members =
            await _unitOfWork
                .CustomerAccountUsers
                .GetAllAsync(
                    filter:
                        x =>
                            x.CustomerAccountId ==
                            accountId,

                    orderBy:
                        query =>
                            query.OrderBy(
                                x => x.Role
                            ),

                    includes:
                    [
                        x => x.User
                    ]
                );


        return Result.Success(
            members
                .Select(MapMember)
                .ToList()
        );
    }


    public async Task<Result<CustomerAccountMemberResponse>>
        AddMemberAsync(
            int accountId,
            int currentUserId,
            AddCustomerAccountMemberRequest request)
    {
        var owner =
            await GetOwnerAsync(
                accountId,
                currentUserId
            );


        if (owner is null)
        {
            return Result
                .Failure<CustomerAccountMemberResponse>(
                    CustomerAccountErrors.AccessDenied
                );
        }


        var phone =
            request.PhoneNumber?.Trim();


        if (string.IsNullOrWhiteSpace(phone))
        {
            return Result
                .Failure<CustomerAccountMemberResponse>(
                    CustomerAccountErrors.UserNotFound
                );
        }


        var user =
            await _userManager
                .Users
                .FirstOrDefaultAsync(
                    x =>
                        x.PhoneNumber == phone
                );


        if (user is null)
        {
            return Result
                .Failure<CustomerAccountMemberResponse>(
                    CustomerAccountErrors.UserNotFound
                );
        }


        var existingMember =
            await _unitOfWork
                .CustomerAccountUsers
                .FirstOrDefaultAsync(
                    x =>
                        x.CustomerAccountId ==
                            accountId
                        &&
                        x.UserId ==
                            user.Id
                );


        if (existingMember is not null)
        {
            if (existingMember.IsActive)
            {
                return Result
                    .Failure<CustomerAccountMemberResponse>(
                        CustomerAccountErrors.MemberAlreadyExists
                    );
            }


            var trackedMember =
                await _unitOfWork
                    .CustomerAccountUsers
                    .FindAsync(
                        existingMember.Id
                    );


            if (trackedMember is null)
            {
                return Result
                    .Failure<CustomerAccountMemberResponse>(
                        CustomerAccountErrors.MemberNotFound
                    );
            }


            trackedMember.IsActive =
                true;

            trackedMember.Role =
                CustomerAccountUserRole.Member;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                new CustomerAccountMemberResponse(
                    trackedMember.Id,
                    user.Id,
                    user.Name,
                    user.PhoneNumber,
                    trackedMember.Role,
                    trackedMember.IsActive
                )
            );
        }


        var member =
            new CustomerAccountUser
            {
                CustomerAccountId =
                    accountId,

                UserId =
                    user.Id,

                Role =
                    CustomerAccountUserRole.Member,

                IsActive =
                    true,

                CreatedAt =
                    DateTime.UtcNow
            };


        await _unitOfWork
            .CustomerAccountUsers
            .InsertAsync(member);


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success(
            new CustomerAccountMemberResponse(
                member.Id,
                user.Id,
                user.Name,
                user.PhoneNumber,
                member.Role,
                member.IsActive
            )
        );
    }


    public async Task<Result> RemoveMemberAsync(
        int accountId,
        int accountUserId,
        int currentUserId)
    {
        var owner =
            await GetOwnerAsync(
                accountId,
                currentUserId
            );


        if (owner is null)
        {
            return Result.Failure(
                CustomerAccountErrors.AccessDenied
            );
        }


        var member =
            await _unitOfWork
                .CustomerAccountUsers
                .GetByIdAsync(
                    accountUserId
                );


        if (member is null
            ||
            member.CustomerAccountId != accountId)
        {
            return Result.Failure(
                CustomerAccountErrors.MemberNotFound
            );
        }


        if (member.Role ==
            CustomerAccountUserRole.Owner)
        {
            return Result.Failure(
                CustomerAccountErrors.OwnerCannotBeRemoved
            );
        }


        var trackedMember =
            await _unitOfWork
                .CustomerAccountUsers
                .FindAsync(member.Id);


        if (trackedMember is null)
        {
            return Result.Failure(
                CustomerAccountErrors.MemberNotFound
            );
        }


        trackedMember.IsActive =
            false;


        /*
         * كمان نعطل كل assignments
         * بتاعته للمشاريع.
         */
        var projectMemberships =
            await _unitOfWork
                .ProjectMembers
                .GetAllAsync(
                    filter:
                        x =>
                            x.CustomerAccountUserId ==
                                member.Id
                            &&
                            x.IsActive
                );


        foreach (var membership in projectMemberships)
        {
            var trackedMembership =
                await _unitOfWork
                    .ProjectMembers
                    .FindAsync(
                        membership.Id
                    );


            if (trackedMembership is not null)
            {
                trackedMembership.IsActive =
                    false;
            }
        }


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    private async Task<CustomerAccountUser?>
        GetOwnerAsync(
            int accountId,
            int userId)
    {
        return await _unitOfWork
            .CustomerAccountUsers
            .FirstOrDefaultAsync(
                x =>
                    x.CustomerAccountId ==
                        accountId
                    &&
                    x.UserId ==
                        userId
                    &&
                    x.Role ==
                        CustomerAccountUserRole.Owner
                    &&
                    x.IsActive
            );
    }


    private async Task<CustomerAccountUser?>
        GetActiveAccountUserAsync(
            int accountId,
            int userId)
    {
        return await _unitOfWork
            .CustomerAccountUsers
            .FirstOrDefaultAsync(
                x =>
                    x.CustomerAccountId ==
                        accountId
                    &&
                    x.UserId ==
                        userId
                    &&
                    x.IsActive
            );
    }


    private static CustomerAccountResponse Map(
        CustomerAccount account)
    {
        return new CustomerAccountResponse(
            account.Id,
            account.Name,
            account.Type,
            account.IsActive,
            account.CreatedAt
        );
    }


    private static CustomerAccountMemberResponse MapMember(
        CustomerAccountUser member)
    {
        return new CustomerAccountMemberResponse(
            member.Id,
            member.UserId,
            member.User.Name,
            member.User.PhoneNumber,
            member.Role,
            member.IsActive
        );
    }
}