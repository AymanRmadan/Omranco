using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.WalletServices;
using Omranco.Domain.Entities.Wallets;
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


        public async Task<Result<int>>
            CreditOrderEarningAsync(
                int orderId)
        {
            // ========================================
            // Order
            // ========================================

            var order =
                await _unitOfWork.Orders
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

            /*
                        if (existingTransaction)
                        {
                            return Result.Failure<int>(WalletErrors.AlreadyCredited);
                        }*/

            if (existingTransaction is not null)
            {
                return Result.Success(existingTransaction.Id);
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
                            order.VendorNetAmount,

                        TotalEarned =
                            order.VendorNetAmount,

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
                                    order.VendorNetAmount,

                                BalanceBefore =
                                    0,

                                BalanceAfter =
                                    order.VendorNetAmount,

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
                balanceBefore +
                order.VendorNetAmount;


            trackedWallet.Balance =
                balanceAfter;

            trackedWallet.TotalEarned +=
                order.VendorNetAmount;

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
                        order.VendorNetAmount,

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
    }
}