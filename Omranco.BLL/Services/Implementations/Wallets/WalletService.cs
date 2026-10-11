using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.WalletServices;
using Omranco.Domain.Entities.Wallets;
using Omranco.Domain.Enums.Payments;
using Omranco.Domain.Enums.Wallets;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.WalletServices
{
    public sealed class VendorWalletService
        : IVendorWalletService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VendorWalletService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Result<int>> CreditOrderEarningAsync(int orderId)
        {
            // ========================================
            // Order
            // ========================================

            var order =
                await _unitOfWork
                    .Orders
                    .GetByIdAsync(orderId);


            if (order is null)
            {
                return Result.Failure<int>(
                    WalletErrors.OrderNotFound
                );
            }


            if (order.VendorNetAmount <= 0)
            {
                return Result.Failure<int>(
                    WalletErrors.InvalidNetAmount
                );
            }


            // ========================================
            // Payment
            // ========================================

            var payment =
                await _unitOfWork
                    .Payments
                    .FirstOrDefaultAsync(
                        x => x.OrderId == orderId
                    );


            if (payment is null)
            {
                return Result.Failure<int>(
                    PaymentErrors.NotFound
                );
            }


            if (payment.PaidAmount <= 0)
            {
                return Result.Failure<int>(
                    WalletErrors.InvalidNetAmount
                );
            }


            // ========================================
            // Calculate current vendor earning
            // after any refunds
            // ========================================

            var remainingPaidAmount =
                payment.PaidAmount
                -
                payment.RefundedAmount;


            /*
             * Payment was fully refunded.
             *
             * Vendor should receive nothing.
             */
            if (remainingPaidAmount <= 0)
            {
                return Result.Success(0);
            }


            /*
             * Example:
             *
             * PaidAmount      = 100,000
             * RefundedAmount  = 20,000
             * VendorNetAmount = 95,000
             *
             * earningAmount =
             * 95,000 * (80,000 / 100,000)
             * = 76,000
             */
            var earningAmount =
                decimal.Round(
                    order.VendorNetAmount
                    *
                    (
                        remainingPaidAmount
                        /
                        payment.PaidAmount
                    ),
                    2,
                    MidpointRounding.AwayFromZero
                );


            if (earningAmount <= 0)
            {
                return Result.Success(0);
            }


            // ========================================
            // Idempotency
            // ========================================

            var reference =
                $"ORDER:{order.Id}:EARNING";


            var existingTransaction =
                await _unitOfWork
                    .VendorWalletTransactions
                    .FirstOrDefaultAsync(
                        x =>
                            x.Reference ==
                            reference
                    );


            if (existingTransaction is not null)
            {
                return Result.Success(
                    existingTransaction.Id
                );
            }


            // ========================================
            // Find Wallet
            // ========================================

            var wallet =
                await _unitOfWork
                    .VendorWallets
                    .FirstOrDefaultAsync(
                        x =>
                            x.VendorId ==
                            order.VendorId
                    );


            var now =
                DateTime.UtcNow;


            // ========================================
            // Create wallet if it doesn't exist
            // ========================================

            if (wallet is null)
            {
                var newWallet =
                    new VendorWallet
                    {
                        VendorId =
                            order.VendorId,

                        Balance =
                            earningAmount,

                        TotalEarned =
                            earningAmount,

                        TotalSettled =
                            0,

                        CreatedAt =
                            now,

                        Transactions =
                        [
                            new VendorWalletTransaction
                    {
                        OrderId =
                            order.Id,

                        Type =
                            VendorWalletTransactionType
                                .OrderEarning,

                        Direction =
                            WalletTransactionDirection
                                .Credit,

                        Amount =
                            earningAmount,

                        BalanceBefore =
                            0,

                        BalanceAfter =
                            earningAmount,

                        Reference =
                            reference,

                        CreatedAt =
                            now
                    }
                        ]
                    };


                await _unitOfWork
                    .VendorWallets
                    .InsertAsync(
                        newWallet
                    );


                await _unitOfWork
                    .SaveChangeAsync();


                var transactionId =
                    newWallet.Transactions
                        .First()
                        .Id;


                return Result.Success(
                    transactionId
                );
            }


            // ========================================
            // Existing Wallet
            // ========================================

            var trackedWallet =
                await _unitOfWork
                    .VendorWallets
                    .FindAsync(
                        wallet.Id
                    );


            if (trackedWallet is null)
            {
                return Result.Failure<int>(
                    WalletErrors.OrderNotFound
                );
            }


            var balanceBefore =
                trackedWallet.Balance;


            var balanceAfter =
                balanceBefore
                +
                earningAmount;


            trackedWallet.Balance =
                balanceAfter;


            trackedWallet.TotalEarned +=
                earningAmount;


            trackedWallet.UpdatedAt =
                now;


            var transaction =
                new VendorWalletTransaction
                {
                    VendorWalletId =
                        trackedWallet.Id,

                    OrderId =
                        order.Id,

                    Type =
                        VendorWalletTransactionType
                            .OrderEarning,

                    Direction =
                        WalletTransactionDirection
                            .Credit,

                    Amount =
                        earningAmount,

                    BalanceBefore =
                        balanceBefore,

                    BalanceAfter =
                        balanceAfter,

                    Reference =
                        reference,

                    CreatedAt =
                        now
                };


            await _unitOfWork
                .VendorWalletTransactions
                .InsertAsync(
                    transaction
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                transaction.Id
            );
        }

        public async Task<Result> ApplyRefundAsync(int paymentRefundId)
        {
            // ==========================================
            // Get successful refund
            // ==========================================

            var refund =
                await _unitOfWork
                    .PaymentRefunds
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == paymentRefundId
                            &&
                            x.Status ==
                                PaymentRefundStatus.Succeeded,
                        x => x.Payment
                    );


            if (refund is null)
            {
                return Result.Failure(
                    PaymentRefundErrors.RefundNotFound
                );
            }


            // ==========================================
            // Get Order
            // ==========================================

            var order =
                await _unitOfWork
                    .Orders
                    .GetByIdAsync(
                        refund.Payment.OrderId
                    );


            if (order is null)
            {
                return Result.Failure(
                    OrderErrors.NotFound
                );
            }


            // ==========================================
            // Was vendor earning credited?
            // ==========================================

            var earningReference =
                $"ORDER:{order.Id}:EARNING";


            var earningTransaction =
                await _unitOfWork
                    .VendorWalletTransactions
                    .FirstOrDefaultAsync(
                        x =>
                            x.OrderId == order.Id
                            &&
                            x.Reference ==
                                earningReference
                            &&
                            x.Type ==
                                VendorWalletTransactionType
                                    .OrderEarning
                            &&
                            x.Direction ==
                                WalletTransactionDirection
                                    .Credit
                    );


            /*
             * لو مفيش earning transaction:
             *
             * غالبًا الـOrder لسه متسلمتش.
             *
             * مش هنخصم حاجة.
             * ولما تتسلم بعدين CreditOrderEarningAsync
             * هتحسب المبلغ المعدل تلقائيًا.
             */
            if (earningTransaction is null)
            {
                return Result.Success();
            }


            // ==========================================
            // Idempotency for THIS refund
            // ==========================================

            var refundReference =
                $"REFUND:{refund.Id}:DEBIT";


            var existingRefundDebit =
                await _unitOfWork
                    .VendorWalletTransactions
                    .FirstOrDefaultAsync(
                        x =>
                            x.Reference ==
                                refundReference
                    );


            if (existingRefundDebit is not null)
            {
                return Result.Success();
            }


            // ==========================================
            // Payment sanity
            // ==========================================

            var payment =
                refund.Payment;


            if (payment.PaidAmount <= 0)
            {
                return Result.Failure(
                    WalletErrors.InvalidNetAmount
                );
            }


            // ==========================================
            // Calculate Target Vendor Earning
            // ==========================================

            var remainingPaidAmount =
                payment.PaidAmount
                -
                payment.RefundedAmount;


            if (remainingPaidAmount < 0)
            {
                return Result.Failure(
                    WalletErrors.InvalidNetAmount
                );
            }


            var targetVendorEarning =
                decimal.Round(
                    order.VendorNetAmount
                    *
                    (
                        remainingPaidAmount
                        /
                        payment.PaidAmount
                    ),
                    2,
                    MidpointRounding.AwayFromZero
                );


            // ==========================================
            // How much was already debited for refunds?
            // ==========================================

            var previousRefundTransactions =
                await _unitOfWork
                    .VendorWalletTransactions
                    .GetAllAsync(
                        filter:
                            x =>
                                x.OrderId == order.Id
                                &&
                                x.Type ==
                                    VendorWalletTransactionType
                                        .Refund
                                &&
                                x.Direction ==
                                    WalletTransactionDirection
                                        .Debit
                    );


            var alreadyRefundedFromVendor =
                previousRefundTransactions.Sum(
                    x => x.Amount
                );


            // ==========================================
            // Current wallet contribution of this Order
            // ==========================================

            var currentVendorEarning =
                earningTransaction.Amount
                -
                alreadyRefundedFromVendor;


            // ==========================================
            // Difference that must be removed now
            // ==========================================

            var debitAmount =
                currentVendorEarning
                -
                targetVendorEarning;


            /*
             * Could already be synchronized.
             */
            if (debitAmount <= 0)
            {
                return Result.Success();
            }


            // ==========================================
            // Wallet
            // ==========================================

            var wallet =
                await _unitOfWork
                    .VendorWallets
                    .FirstOrDefaultAsync(
                        x => x.VendorId == order.VendorId
                    );


            if (wallet is null)
            {
                return Result.Failure(
                    WalletErrors.OrderNotFound
                );
            }


            var trackedWallet =
                await _unitOfWork
                    .VendorWallets
                    .FindAsync(
                        wallet.Id
                    );


            if (trackedWallet is null)
            {
                return Result.Failure(
                    WalletErrors.OrderNotFound
                );
            }


            var balanceBefore =
                trackedWallet.Balance;


            var balanceAfter =
                balanceBefore
                -
                debitAmount;


            // ==========================================
            // Important:
            // Allow negative balance
            // ==========================================

            trackedWallet.Balance =
                balanceAfter;

            trackedWallet.UpdatedAt =
                DateTime.UtcNow;


            // ==========================================
            // Wallet Transaction
            // ==========================================

            var walletTransaction =
                new VendorWalletTransaction
                {
                    VendorWalletId =
                        trackedWallet.Id,

                    OrderId =
                        order.Id,

                    Type =
                        VendorWalletTransactionType
                            .Refund,

                    Direction =
                        WalletTransactionDirection
                            .Debit,

                    Amount =
                        debitAmount,

                    BalanceBefore =
                        balanceBefore,

                    BalanceAfter =
                        balanceAfter,

                    Reference =
                        refundReference,

                    Notes =
                        $"Refund #{refund.Id} for order {order.Number}",

                    CreatedAt =
                        DateTime.UtcNow
                };


            await _unitOfWork
                .VendorWalletTransactions
                .InsertAsync(
                    walletTransaction
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }
    }
}