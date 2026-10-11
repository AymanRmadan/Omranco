using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.PayoutAccounts;
using Omranco.BLL.Services.Abstractions.VendorServices;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Enums.Vendors;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.VendorServices;

public sealed class VendorPayoutAccountService
    : IVendorPayoutAccountService
{
    private readonly IUnitOfWork _unitOfWork;

    public VendorPayoutAccountService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    // =========================================================
    // Get My Accounts
    // =========================================================

    public async Task<Result<List<VendorPayoutAccountResponse>>>
        GetMyAccountsAsync(
            int currentUserId)
    {
        var vendor =
            await GetVendorAsync(
                currentUserId
            );


        if (vendor is null)
        {
            return Result
                .Failure<List<VendorPayoutAccountResponse>>(
                    VendorPayoutAccountErrors.VendorNotFound
                );
        }


        var accounts =
            await _unitOfWork
                .VendorPayoutAccounts
                .GetAllAsync(
                    filter:
                        x => x.VendorId == vendor.Id,

                    orderBy:
                        query =>
                            query
                                .OrderByDescending(
                                    x => x.IsDefault
                                )
                                .ThenByDescending(
                                    x => x.CreatedAt
                                )
                );


        return Result.Success(
            accounts
                .Select(Map)
                .ToList()
        );
    }


    // =========================================================
    // Create
    // =========================================================
    public async Task<Result<VendorPayoutAccountResponse>>
        CreateAsync(
            int currentUserId,
            CreateVendorPayoutAccountRequest request)
    {
        var vendor =
            await GetVendorAsync(
                currentUserId
            );


        if (vendor is null)
        {
            return Result
                .Failure<VendorPayoutAccountResponse>(
                    VendorPayoutAccountErrors.VendorNotFound
                );
        }


        var validationResult =
            ValidateAccount(
                request.Method,
                request.AccountHolderName,
                request.BankName,
                request.AccountNumber,
                request.Iban,
                request.WalletProvider,
                request.WalletPhoneNumber
            );


        if (validationResult.IsFailure)
        {
            return Result
                .Failure<VendorPayoutAccountResponse>(
                    validationResult.Error
                );
        }


        var existingAccounts =
            await _unitOfWork
                .VendorPayoutAccounts
                .GetAllAsync(
                    filter:
                        x => x.VendorId == vendor.Id
                );


        /*
         * أول حساب يتعمل Default تلقائيًا.
         */
        var shouldBeDefault =
            existingAccounts.Count == 0
            ||
            request.IsDefault;


        /*
         * لو الحساب الجديد Default:
         * نشيل Default من الحساب القديم.
         */
        if (shouldBeDefault)
        {
            foreach (var existingAccount in existingAccounts)
            {
                if (!existingAccount.IsDefault)
                    continue;


                var trackedAccount =
                    await _unitOfWork
                        .VendorPayoutAccounts
                        .FindAsync(
                            existingAccount.Id
                        );


                if (trackedAccount is not null)
                {
                    trackedAccount.IsDefault =
                        false;
                }
            }
        }


        var newAccount =
            new VendorPayoutAccount
            {
                VendorId =
                    vendor.Id,

                Method =
                    request.Method,

                AccountHolderName =
                    Normalize(
                        request.AccountHolderName
                    ),

                BankName =
                    request.Method ==
                        PayoutMethod.BankTransfer
                            ? Normalize(
                                request.BankName
                            )
                            : null,

                AccountNumber =
                    request.Method ==
                        PayoutMethod.BankTransfer
                            ? Normalize(
                                request.AccountNumber
                            )
                            : null,

                Iban =
                    request.Method ==
                        PayoutMethod.BankTransfer
                            ? Normalize(
                                request.Iban
                            )
                            : null,

                WalletProvider =
                    request.Method ==
                        PayoutMethod.Wallet
                            ? Normalize(
                                request.WalletProvider
                            )
                            : null,

                WalletPhoneNumber =
                    request.Method ==
                        PayoutMethod.Wallet
                            ? Normalize(
                                request.WalletPhoneNumber
                            )
                            : null,

                IsDefault =
                    shouldBeDefault,

                IsVerified =
                    false,

                CreatedAt =
                    DateTime.UtcNow
            };


        await _unitOfWork
            .VendorPayoutAccounts
            .InsertAsync(
                newAccount
            );


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success(
            Map(newAccount)
        );
    }

    // =========================================================
    // Update
    // =========================================================

    public async Task<Result<VendorPayoutAccountResponse>>
        UpdateAsync(
            int accountId,
            int currentUserId,
            UpdateVendorPayoutAccountRequest request)
    {
        var vendor =
            await GetVendorAsync(
                currentUserId
            );


        if (vendor is null)
        {
            return Result
                .Failure<VendorPayoutAccountResponse>(
                    VendorPayoutAccountErrors.VendorNotFound
                );
        }


        var account =
            await _unitOfWork
                .VendorPayoutAccounts
                .GetByIdAsync(
                    accountId
                );


        if (account is null)
        {
            return Result
                .Failure<VendorPayoutAccountResponse>(
                    VendorPayoutAccountErrors.NotFound
                );
        }


        if (account.VendorId !=
            vendor.Id)
        {
            return Result
                .Failure<VendorPayoutAccountResponse>(
                    VendorPayoutAccountErrors.AccessDenied
                );
        }


        var validationResult =
            ValidateAccount(
                request.Method,
                request.AccountHolderName,
                request.BankName,
                request.AccountNumber,
                request.Iban,
                request.WalletProvider,
                request.WalletPhoneNumber
            );


        if (validationResult.IsFailure)
        {
            return Result
                .Failure<VendorPayoutAccountResponse>(
                    validationResult.Error
                );
        }


        var trackedAccount =
            await _unitOfWork
                .VendorPayoutAccounts
                .FindAsync(
                    account.Id
                );


        if (trackedAccount is null)
        {
            return Result
                .Failure<VendorPayoutAccountResponse>(
                    VendorPayoutAccountErrors.NotFound
                );
        }


        trackedAccount.Method =
            request.Method;

        trackedAccount.AccountHolderName =
            Normalize(
                request.AccountHolderName
            );


        // ==========================================
        // Bank
        // ==========================================

        if (request.Method ==
            PayoutMethod.BankTransfer)
        {
            trackedAccount.BankName =
                Normalize(
                    request.BankName
                );

            trackedAccount.AccountNumber =
                Normalize(
                    request.AccountNumber
                );

            trackedAccount.Iban =
                Normalize(
                    request.Iban
                );


            trackedAccount.WalletProvider =
                null;

            trackedAccount.WalletPhoneNumber =
                null;
        }


        // ==========================================
        // Wallet
        // ==========================================

        if (request.Method ==
            PayoutMethod.Wallet)
        {
            trackedAccount.WalletProvider =
                Normalize(
                    request.WalletProvider
                );

            trackedAccount.WalletPhoneNumber =
                Normalize(
                    request.WalletPhoneNumber
                );


            trackedAccount.BankName =
                null;

            trackedAccount.AccountNumber =
                null;

            trackedAccount.Iban =
                null;
        }


        /*
         * أي تعديل مالي يلغي Verification السابقة.
         */
        trackedAccount.IsVerified =
            false;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success(
            Map(trackedAccount)
        );
    }


    // =========================================================
    // Set Default
    // =========================================================

    public async Task<Result> SetDefaultAsync(
        int accountId,
        int currentUserId)
    {
        var vendor =
            await GetVendorAsync(
                currentUserId
            );


        if (vendor is null)
        {
            return Result.Failure(
                VendorPayoutAccountErrors.VendorNotFound
            );
        }


        var account =
            await _unitOfWork
                .VendorPayoutAccounts
                .GetByIdAsync(
                    accountId
                );


        if (account is null)
        {
            return Result.Failure(
                VendorPayoutAccountErrors.NotFound
            );
        }


        if (account.VendorId !=
            vendor.Id)
        {
            return Result.Failure(
                VendorPayoutAccountErrors.AccessDenied
            );
        }


        if (account.IsDefault)
        {
            return Result.Success();
        }


        var accounts =
            await _unitOfWork
                .VendorPayoutAccounts
                .GetAllAsync(
                    filter:
                        x =>
                            x.VendorId ==
                            vendor.Id
                );


        foreach (var currentAccount in accounts)
        {
            if (currentAccount.Id ==
                account.Id)
            {
                continue;
            }


            if (!currentAccount.IsDefault)
            {
                continue;
            }


            var trackedCurrent =
                await _unitOfWork
                    .VendorPayoutAccounts
                    .FindAsync(
                        currentAccount.Id
                    );


            if (trackedCurrent is not null)
            {
                trackedCurrent.IsDefault =
                    false;
            }
        }


        var trackedAccount =
            await _unitOfWork
                .VendorPayoutAccounts
                .FindAsync(
                    account.Id
                );


        if (trackedAccount is null)
        {
            return Result.Failure(
                VendorPayoutAccountErrors.NotFound
            );
        }


        trackedAccount.IsDefault =
            true;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    // =========================================================
    // Admin Verify
    // =========================================================

    public async Task<Result> VerifyAsync(
        int accountId)
    {
        var account =
            await _unitOfWork
                .VendorPayoutAccounts
                .GetByIdAsync(
                    accountId
                );


        if (account is null)
        {
            return Result.Failure(
                VendorPayoutAccountErrors.NotFound
            );
        }


        if (account.IsVerified)
        {
            return Result.Success();
        }


        /*
         * نعمل validation مرة تانية وقت Verification.
         */
        var validationResult =
            ValidateAccount(
                account.Method,
                account.AccountHolderName,
                account.BankName,
                account.AccountNumber,
                account.Iban,
                account.WalletProvider,
                account.WalletPhoneNumber
            );


        if (validationResult.IsFailure)
        {
            return validationResult;
        }


        var trackedAccount =
            await _unitOfWork
                .VendorPayoutAccounts
                .FindAsync(
                    account.Id
                );


        if (trackedAccount is null)
        {
            return Result.Failure(
                VendorPayoutAccountErrors.NotFound
            );
        }


        trackedAccount.IsVerified =
            true;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    // =========================================================
    // Helpers
    // =========================================================

    private async Task<Vendor?> GetVendorAsync(
        int currentUserId)
    {
        return await _unitOfWork
            .Vendors
            .FirstOrDefaultAsync(
                x =>
                    x.OwnerUserId ==
                    currentUserId
                    &&
                    x.IsActive
            );
    }


    private static Result ValidateAccount(
        PayoutMethod method,
        string? accountHolderName,
        string? bankName,
        string? accountNumber,
        string? iban,
        string? walletProvider,
        string? walletPhoneNumber)
    {
        if (method ==
            PayoutMethod.BankTransfer)
        {
            if (
                string.IsNullOrWhiteSpace(
                    accountHolderName)
                ||
                string.IsNullOrWhiteSpace(
                    bankName)
                ||
                (
                    string.IsNullOrWhiteSpace(
                        accountNumber)
                    &&
                    string.IsNullOrWhiteSpace(
                        iban)
                )
            )
            {
                return Result.Failure(
                    VendorPayoutAccountErrors
                        .InvalidBankAccount
                );
            }


            return Result.Success();
        }


        if (method ==
            PayoutMethod.Wallet)
        {
            if (
                string.IsNullOrWhiteSpace(
                    walletProvider)
                ||
                string.IsNullOrWhiteSpace(
                    walletPhoneNumber)
            )
            {
                return Result.Failure(
                    VendorPayoutAccountErrors
                        .InvalidWallet
                );
            }


            return Result.Success();
        }


        return Result.Failure(
            VendorPayoutAccountErrors
                .InvalidBankAccount
        );
    }


    private static string? Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }


    private static VendorPayoutAccountResponse Map(
        VendorPayoutAccount account)
    {
        return new VendorPayoutAccountResponse(
            Id:
                account.Id,

            Method:
                account.Method,

            AccountHolderName:
                account.AccountHolderName,

            BankName:
                account.BankName,

            AccountNumber:
                account.AccountNumber,

            Iban:
                account.Iban,

            WalletProvider:
                account.WalletProvider,

            WalletPhoneNumber:
                account.WalletPhoneNumber,

            IsDefault:
                account.IsDefault,

            IsVerified:
                account.IsVerified,

            CreatedAt:
                account.CreatedAt
        );
    }
}