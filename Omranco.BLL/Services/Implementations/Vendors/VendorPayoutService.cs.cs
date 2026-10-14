using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Vendors.Payouts;
using Omranco.BLL.Services.Abstractions.VendorServices;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Entities.Wallets;
using Omranco.Domain.Enums.Vendors;
using Omranco.Domain.Enums.Wallets;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.VendorServices;

public sealed class VendorPayoutService
    : IVendorPayoutService
{
    private readonly IUnitOfWork _unitOfWork;

    public VendorPayoutService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }



    // =========================================================
    // Vendor - Wallet Summary
    // =========================================================

    public async Task<Result<VendorWalletSummaryResponse>> GetWalletSummaryAsync(int currentUserId)
    {
        var vendor =
            await GetVendorAsync(currentUserId);

        if (vendor is null)
        {
            return Result
                .Failure<VendorWalletSummaryResponse>(
                    VendorPayoutErrors.VendorNotFound
                );
        }


        var wallet =
            await _unitOfWork
                .VendorWallets
                .FirstOrDefaultAsync(
                    x => x.VendorId == vendor.Id
                );


        if (wallet is null)
        {
            return Result.Success(
                new VendorWalletSummaryResponse(
                    Balance: 0,
                    ReservedAmount: 0,
                    AvailableToWithdraw: 0,
                    TotalEarned: 0,
                    TotalSettled: 0
                )
            );
        }


        var reservedAmount =
            await GetReservedAmountAsync(
                vendor.Id
            );


        var available =
            Math.Max(
                0m,
                wallet.Balance
                -
                reservedAmount
            );


        return Result.Success(
            new VendorWalletSummaryResponse(
                Balance:
                    wallet.Balance,

                ReservedAmount:
                    reservedAmount,

                AvailableToWithdraw:
                    available,

                TotalEarned:
                    wallet.TotalEarned,

                TotalSettled:
                    wallet.TotalSettled
            )
        );
    }


    // =========================================================
    // Vendor - My Payouts
    // =========================================================

    public async Task<Result<List<VendorPayoutResponse>>>
        GetMyPayoutsAsync(
            int currentUserId)
    {
        var vendor =
            await GetVendorAsync(currentUserId);

        if (vendor is null)
        {
            return Result
                .Failure<List<VendorPayoutResponse>>(
                    VendorPayoutErrors.VendorNotFound
                );
        }


        var payouts =
            await _unitOfWork
                .VendorPayoutRequests
                .GetAllAsync(
                    filter:
                        x => x.VendorId == vendor.Id,

                    orderBy:
                        query =>
                            query.OrderByDescending(
                                x => x.RequestedAt
                            )
                );


        return Result.Success(
            payouts
                .Select(Map)
                .ToList()
        );
    }


    // =========================================================
    // Vendor - Request Payout
    // =========================================================

    public async Task<Result<VendorPayoutResponse>> RequestAsync(int currentUserId, CreateVendorPayoutRequest request)
    {
        if (request.Amount <= 0)
        {
            return Result
                .Failure<VendorPayoutResponse>(
                    VendorPayoutErrors.InvalidAmount
                );
        }


        var vendor =
            await GetVendorAsync(currentUserId);

        if (vendor is null)
        {
            return Result
                .Failure<VendorPayoutResponse>(
                    VendorPayoutErrors.VendorNotFound
                );
        }


        // ==========================================
        // Payout Account
        // ==========================================

        var payoutAccount =
            await _unitOfWork
                .VendorPayoutAccounts
                .FirstOrDefaultAsync(
                    x =>
                        x.Id ==
                            request.VendorPayoutAccountId
                        &&
                        x.VendorId ==
                            vendor.Id
                );


        if (payoutAccount is null)
        {
            return Result
                .Failure<VendorPayoutResponse>(
                    VendorPayoutErrors
                        .PayoutAccountNotFound
                );
        }


        if (!payoutAccount.IsVerified)
        {
            return Result
                .Failure<VendorPayoutResponse>(
                    VendorPayoutErrors
                        .PayoutAccountNotVerified
                );
        }


        if (!IsValidPayoutAccount(
                payoutAccount))
        {
            return Result
                .Failure<VendorPayoutResponse>(
                    VendorPayoutErrors
                        .InvalidPayoutAccount
                );
        }


        // ==========================================
        // Wallet
        // ==========================================

        var wallet =
            await _unitOfWork
                .VendorWallets
                .FirstOrDefaultAsync(
                    x =>
                        x.VendorId ==
                            vendor.Id
                );


        if (wallet is null)
        {
            return Result
                .Failure<VendorPayoutResponse>(
                    VendorPayoutErrors.WalletNotFound
                );
        }


        // ==========================================
        // Reserved Amount
        // ==========================================

        var reservedAmount =
            await GetReservedAmountAsync(
                vendor.Id
            );


        var available =
            wallet.Balance
            -
            reservedAmount;


        if (available <= 0
            ||
            request.Amount > available)
        {
            return Result
                .Failure<VendorPayoutResponse>(
                    VendorPayoutErrors
                        .InsufficientAvailableBalance
                );
        }


        // ==========================================
        // Create Request
        // ==========================================

        var now =
            DateTime.UtcNow;


        var payout =
    new VendorPayoutRequest
    {
        VendorId =
            vendor.Id,

        VendorPayoutAccountId =
            payoutAccount.Id,

        Amount =
            request.Amount,

        Status =
            VendorPayoutRequestStatus.Pending,

        Reference =
            $"PAYOUT-{vendor.Id}-{Guid.NewGuid():N}",


        // ==========================================
        // Payout Account Snapshot
        // ==========================================

        PayoutMethod =
            payoutAccount.Method,

        AccountHolderName =
            payoutAccount.AccountHolderName,

        BankName =
            payoutAccount.BankName,

        AccountNumber =
            payoutAccount.AccountNumber,

        IBAN =
            payoutAccount.Iban,

        WalletProvider =
            payoutAccount.WalletProvider,

        WalletPhoneNumber =
            payoutAccount.WalletPhoneNumber,


        RequestedAt =
            now
    };

        await _unitOfWork
            .VendorPayoutRequests
            .InsertAsync(
                payout
            );


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success(
            Map(payout)
        );
    }


    // =========================================================
    // Vendor - Cancel Pending Request
    // =========================================================

    public async Task<Result> CancelAsync(
        int payoutRequestId,
        int currentUserId)
    {
        var vendor =
            await GetVendorAsync(
                currentUserId
            );


        if (vendor is null)
        {
            return Result.Failure(
                VendorPayoutErrors.VendorNotFound
            );
        }


        var payout =
            await _unitOfWork
                .VendorPayoutRequests
                .GetByIdAsync(
                    payoutRequestId
                );


        if (payout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        if (payout.VendorId !=
            vendor.Id)
        {
            return Result.Failure(
                VendorPayoutErrors.AccessDenied
            );
        }


        if (payout.Status !=
            VendorPayoutRequestStatus.Pending)
        {
            return Result.Failure(
                VendorPayoutErrors.InvalidStatus
            );
        }


        var trackedPayout =
            await _unitOfWork
                .VendorPayoutRequests
                .FindAsync(
                    payout.Id
                );


        if (trackedPayout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        var now =
            DateTime.UtcNow;


        trackedPayout.Status =
            VendorPayoutRequestStatus.Cancelled;

        trackedPayout.CompletedAt =
            now;

        trackedPayout.UpdatedAt =
            now;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    // =========================================================
    // Admin - List
    // =========================================================

    public async Task<Result<List<VendorPayoutResponse>>>
        GetAllAsync(
            VendorPayoutRequestStatus? status)
    {
        List<VendorPayoutRequest> payouts;


        if (status.HasValue)
        {
            payouts =
                await _unitOfWork
                    .VendorPayoutRequests
                    .GetAllAsync(
                        filter:
                            x =>
                                x.Status ==
                                status.Value,

                        orderBy:
                            query =>
                                query.OrderByDescending(
                                    x => x.RequestedAt
                                )
                    );
        }
        else
        {
            payouts =
                await _unitOfWork
                    .VendorPayoutRequests
                    .GetAllAsync(
                        orderBy:
                            query =>
                                query.OrderByDescending(
                                    x => x.RequestedAt
                                )
                    );
        }


        return Result.Success(
            payouts
                .Select(Map)
                .ToList()
        );
    }


    // =========================================================
    // Admin - Approve
    // =========================================================

    public async Task<Result> ApproveAsync(
        int payoutRequestId,
        int currentUserId,
        ApproveVendorPayoutRequest request)
    {
        var payout =
            await _unitOfWork
                .VendorPayoutRequests
                .GetByIdAsync(
                    payoutRequestId
                );


        if (payout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        if (payout.Status !=
            VendorPayoutRequestStatus.Pending)
        {
            return Result.Failure(
                VendorPayoutErrors.InvalidStatus
            );
        }


        var trackedPayout =
            await _unitOfWork
                .VendorPayoutRequests
                .FindAsync(
                    payout.Id
                );


        if (trackedPayout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        var now =
            DateTime.UtcNow;


        trackedPayout.Status =
            VendorPayoutRequestStatus.Approved;

        trackedPayout.ReviewedByUserId =
            currentUserId;

        trackedPayout.ReviewNotes =
            string.IsNullOrWhiteSpace(
                request.ReviewNotes)
                ? null
                : request.ReviewNotes.Trim();

        trackedPayout.ReviewedAt =
            now;

        trackedPayout.UpdatedAt =
            now;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    // =========================================================
    // Admin - Reject
    // =========================================================

    public async Task<Result> RejectAsync(
        int payoutRequestId,
        int currentUserId,
        RejectVendorPayoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.ReviewNotes))
        {
            return Result.Failure(
                VendorPayoutErrors
                    .ReviewNotesRequired
            );
        }


        var payout =
            await _unitOfWork
                .VendorPayoutRequests
                .GetByIdAsync(
                    payoutRequestId
                );


        if (payout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        if (payout.Status !=
            VendorPayoutRequestStatus.Pending)
        {
            return Result.Failure(
                VendorPayoutErrors.InvalidStatus
            );
        }


        var trackedPayout =
            await _unitOfWork
                .VendorPayoutRequests
                .FindAsync(
                    payout.Id
                );


        if (trackedPayout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        var now =
            DateTime.UtcNow;


        trackedPayout.Status =
            VendorPayoutRequestStatus.Rejected;

        trackedPayout.ReviewedByUserId =
            currentUserId;

        trackedPayout.ReviewNotes =
            request.ReviewNotes.Trim();

        trackedPayout.ReviewedAt =
            now;

        trackedPayout.CompletedAt =
            now;

        trackedPayout.UpdatedAt =
            now;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    // =========================================================
    // Admin - Start Processing
    // =========================================================

    public async Task<Result> StartProcessingAsync(
        int payoutRequestId,
        ProcessVendorPayoutRequest request)
    {
        var payout =
            await _unitOfWork
                .VendorPayoutRequests
                .GetByIdAsync(
                    payoutRequestId
                );


        if (payout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        /*
         * Failed payout may be retried.
         */
        if (payout.Status !=
                VendorPayoutRequestStatus.Approved
            &&
            payout.Status !=
                VendorPayoutRequestStatus.Failed)
        {
            return Result.Failure(
                VendorPayoutErrors.InvalidStatus
            );
        }


        var wallet =
            await _unitOfWork
                .VendorWallets
                .FirstOrDefaultAsync(
                    x =>
                        x.VendorId ==
                            payout.VendorId
                );


        if (wallet is null)
        {
            return Result.Failure(
                VendorPayoutErrors.WalletNotFound
            );
        }


        /*
         * Re-check available balance.
         *
         * Especially important when retrying
         * a previously Failed request.
         */
        var reservedByOtherRequests =
            await GetReservedAmountAsync(
                payout.VendorId,
                payout.Id
            );


        var available =
            wallet.Balance
            -
            reservedByOtherRequests;


        if (available <
            payout.Amount)
        {
            return Result.Failure(
                VendorPayoutErrors
                    .InsufficientAvailableBalance
            );
        }


        var trackedPayout =
            await _unitOfWork
                .VendorPayoutRequests
                .FindAsync(
                    payout.Id
                );


        if (trackedPayout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        var now =
            DateTime.UtcNow;


        trackedPayout.Status =
            VendorPayoutRequestStatus.Processing;

        trackedPayout.ExternalReference =
            string.IsNullOrWhiteSpace(
                request.ExternalReference)
                ? null
                : request.ExternalReference.Trim();

        trackedPayout.FailureReason =
            null;

        trackedPayout.ProcessingAt =
            now;

        trackedPayout.CompletedAt =
            null;

        trackedPayout.UpdatedAt =
            now;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    // =========================================================
    // Admin - Paid
    // =========================================================

    public async Task<Result> MarkPaidAsync(
        int payoutRequestId)
    {
        var payout =
            await _unitOfWork
                .VendorPayoutRequests
                .GetByIdAsync(
                    payoutRequestId
                );


        if (payout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        // ==========================================
        // Idempotency
        // ==========================================

        var settlementReference =
            $"PAYOUT:{payout.Id}:SETTLEMENT";


        var existingSettlement =
            await _unitOfWork
                .VendorWalletTransactions
                .FirstOrDefaultAsync(
                    x =>
                        x.Reference ==
                            settlementReference
                );


        if (existingSettlement is not null)
        {
            /*
             * Wallet was already debited.
             *
             * Reconcile payout status if needed.
             */
            if (payout.Status !=
                VendorPayoutRequestStatus.Paid)
            {
                var trackedPayout =
                    await _unitOfWork
                        .VendorPayoutRequests
                        .FindAsync(
                            payout.Id
                        );


                if (trackedPayout is not null)
                {
                    var naw =
                        DateTime.UtcNow;

                    trackedPayout.Status =
                        VendorPayoutRequestStatus.Paid;

                    trackedPayout.CompletedAt ??=
                        naw;

                    trackedPayout.UpdatedAt =
                        naw;


                    await _unitOfWork.SaveChangeAsync();
                }
            }


            return Result.Success();
        }


        if (payout.Status !=
            VendorPayoutRequestStatus.Processing)
        {
            return Result.Failure(
                VendorPayoutErrors.InvalidStatus
            );
        }


        // ==========================================
        // Wallet
        // ==========================================

        var wallet =
            await _unitOfWork
                .VendorWallets
                .FirstOrDefaultAsync(
                    x =>
                        x.VendorId ==
                            payout.VendorId
                );


        if (wallet is null)
        {
            return Result.Failure(
                VendorPayoutErrors.WalletNotFound
            );
        }


        var trackedWallet =
            await _unitOfWork
                .VendorWallets
                .FindAsync(
                    wallet.Id
                );


        var trackedPayoutRequest =
            await _unitOfWork
                .VendorPayoutRequests
                .FindAsync(
                    payout.Id
                );


        if (trackedWallet is null
            ||
            trackedPayoutRequest is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        var now =
            DateTime.UtcNow;

        var balanceBefore =
            trackedWallet.Balance;

        var balanceAfter =
            balanceBefore
            -
            payout.Amount;


        /*
         * Negative balance is allowed.
         *
         * A refund may have reduced the wallet
         * after payout processing had already begun.
         */
        trackedWallet.Balance =
            balanceAfter;

        trackedWallet.TotalSettled +=
            payout.Amount;

        trackedWallet.UpdatedAt =
            now;


        // ==========================================
        // Settlement Transaction
        // ==========================================

        var transaction =
            new VendorWalletTransaction
            {
                VendorWalletId =
                    trackedWallet.Id,

                OrderId =
                    null,

                Type =
                    VendorWalletTransactionType
                        .Settlement,

                Direction =
                    WalletTransactionDirection
                        .Debit,

                Amount =
                    payout.Amount,

                BalanceBefore =
                    balanceBefore,

                BalanceAfter =
                    balanceAfter,

                Reference =
                    settlementReference,

                Notes =
                    $"Payout {payout.Reference}",

                CreatedAt =
                    now
            };


        await _unitOfWork
            .VendorWalletTransactions
            .InsertAsync(
                transaction
            );


        // ==========================================
        // Complete Payout
        // ==========================================

        trackedPayoutRequest.Status =
            VendorPayoutRequestStatus.Paid;

        trackedPayoutRequest.CompletedAt =
            now;

        trackedPayoutRequest.UpdatedAt =
            now;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    // =========================================================
    // Admin - Failed
    // =========================================================

    public async Task<Result> MarkFailedAsync(
        int payoutRequestId,
        FailVendorPayoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.FailureReason))
        {
            return Result.Failure(
                VendorPayoutErrors
                    .FailureReasonRequired
            );
        }


        var payout =
            await _unitOfWork
                .VendorPayoutRequests
                .GetByIdAsync(
                    payoutRequestId
                );


        if (payout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        if (payout.Status !=
            VendorPayoutRequestStatus.Processing)
        {
            return Result.Failure(
                VendorPayoutErrors.InvalidStatus
            );
        }


        var trackedPayout =
            await _unitOfWork
                .VendorPayoutRequests
                .FindAsync(
                    payout.Id
                );


        if (trackedPayout is null)
        {
            return Result.Failure(
                VendorPayoutErrors.NotFound
            );
        }


        var now =
            DateTime.UtcNow;


        trackedPayout.Status =
            VendorPayoutRequestStatus.Failed;

        trackedPayout.FailureReason =
            request.FailureReason.Trim();

        trackedPayout.CompletedAt =
            now;

        trackedPayout.UpdatedAt =
            now;


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
            );
    }


    private async Task<decimal> GetReservedAmountAsync(
        int vendorId,
        int? excludePayoutId = null)
    {
        var activePayouts =
            await _unitOfWork
                .VendorPayoutRequests
                .GetAllAsync(
                    filter:
                        x =>
                            x.VendorId ==
                                vendorId
                            &&
                            (!excludePayoutId.HasValue
                             ||
                             x.Id !=
                                excludePayoutId.Value)
                            &&
                            (
                                x.Status ==
                                    VendorPayoutRequestStatus
                                        .Pending
                                ||
                                x.Status ==
                                    VendorPayoutRequestStatus
                                        .Approved
                                ||
                                x.Status ==
                                    VendorPayoutRequestStatus
                                        .Processing
                            )
                );


        return activePayouts.Sum(
            x => x.Amount
        );
    }


    private static bool IsValidPayoutAccount(
        VendorPayoutAccount account)
    {
        return account.Method switch
        {
            PayoutMethod.BankTransfer =>
                !string.IsNullOrWhiteSpace(
                    account.AccountHolderName)
                &&
                !string.IsNullOrWhiteSpace(
                    account.BankName)
                &&
                (
                    !string.IsNullOrWhiteSpace(
                        account.AccountNumber)
                    ||
                    !string.IsNullOrWhiteSpace(
                        account.Iban)
                ),

            PayoutMethod.Wallet =>
                !string.IsNullOrWhiteSpace(
                    account.WalletProvider)
                &&
                !string.IsNullOrWhiteSpace(
                    account.WalletPhoneNumber),

            _ => false
        };
    }


    private static VendorPayoutResponse Map(VendorPayoutRequest payout)
    {
        return new VendorPayoutResponse(
            Id:
                payout.Id,

            Reference:
                payout.Reference,

            Amount:
                payout.Amount,

            Status:
                payout.Status,

            PayoutMethod:
                payout.PayoutMethod,

            AccountHolderName:
                payout.AccountHolderName,

            BankName:
                payout.BankName,

            IBAN:
                payout.IBAN,

            WalletProvider:
                payout.WalletProvider,

            WalletPhoneNumber:
                payout.WalletPhoneNumber,

            ExternalReference:
                payout.ExternalReference,

            ReviewNotes:
                payout.ReviewNotes,

            FailureReason:
                payout.FailureReason,

            RequestedAt:
                payout.RequestedAt,

            ReviewedAt:
                payout.ReviewedAt,

            ProcessingAt:
                payout.ProcessingAt,

            CompletedAt:
                payout.CompletedAt
        );
    }
}